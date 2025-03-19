using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class ChatHub : Hub
{
    // Track active support staff (connectionId -> connectionId)
    private static readonly ConcurrentDictionary<string, string> _supportStaff = new();

    // Track user connections (userId -> connectionId)
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();

    // Register as support staff
    public async Task RegisterAsSupport()
    {
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"RegisterAsSupport called: {connectionId}");

        _supportStaff[connectionId] = connectionId;
        await Clients.Caller.SendAsync("RegisteredAsSupport");

        Console.WriteLine($"Current support staff count: {_supportStaff.Count}");
    }

    // Register user connection
    public async Task RegisterUser(string userId)
    {
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"RegisterUser called: userId={userId}, connectionId={connectionId}");

        _userConnections[userId] = connectionId;
        await Clients.Caller.SendAsync("RegisteredUser");

        Console.WriteLine($"Current user connections count: {_userConnections.Count}");
    }

    // Send message from user to support
    public async Task SendMessage(string userId, string message, string userType)
    {
        Console.WriteLine($"SendMessage called: userId={userId}, message={message}, userType={userType}");

        // Save in memory (you might want to persist this to a database)

        // Send back to caller for confirmation
        //await Clients.Caller.SendAsync("ReceiveMessage", message, userType);
        Console.WriteLine($"Message sent back to caller: {Context.ConnectionId}");

        // Notify all support staff about this message
        foreach (var supportId in _supportStaff.Keys)
        {
            Console.WriteLine($"Sending to support staff: {supportId}");
            await Clients.Client(supportId).SendAsync("ReceiveSupportMessage", userId, message, userType);
        }

        // For new chat sessions, notify support
        if (userType == "user" && !string.IsNullOrEmpty(message))
        {
            Console.WriteLine($"New chat session detected, notifying support staff");
            foreach (var supportId in _supportStaff.Keys)
            {
                await Clients.Client(supportId).SendAsync("NewChatSession", userId, null);
            }
        }
    }

    // Send message from support to user
    public async Task SendSupportMessage(string userId, string message)
    {
        Console.WriteLine($"SendSupportMessage called: userId={userId}, message={message}");

        // Try to find the user's connection
        if (_userConnections.TryGetValue(userId, out string userConnectionId))
        {
            Console.WriteLine($"Found user connection: {userConnectionId} for userId: {userId}");
            // Send message to the specific user via their connection ID
            await Clients.Client(userConnectionId).SendAsync("ReceiveMessage", message, "support");
        }
        else
        {
            Console.WriteLine($"User connection not found for userId: {userId}");
        }

        // Also send to all support staff so they can see each other's messages
        foreach (var supportId in _supportStaff.Keys)
        {
            Console.WriteLine($"Sending support message to staff: {supportId}");
            await Clients.Client(supportId).SendAsync("ReceiveSupportMessage", userId, message, "support");
        }
    }

    // Get all active chats for support staff
    public async Task GetActiveChats()
    {
        Console.WriteLine($"GetActiveChats called by: {Context.ConnectionId}");

        // In a real implementation, you would get this from a database
        // This is just a placeholder that returns the current active connections
        var activeUsers = _userConnections.Keys.ToList();

        await Clients.Caller.SendAsync("ActiveChats", activeUsers);
        Console.WriteLine($"Sent {activeUsers.Count} active chats to: {Context.ConnectionId}");
    }

    // Get connection statistics 
    public async Task GetConnectionStats()
    {
        int userCount = _userConnections.Count;
        int supportCount = _supportStaff.Count;

        Console.WriteLine($"Connection stats: Active users={userCount}, Active support={supportCount}");

        await Clients.Caller.SendAsync("ConnectionStats", userCount, supportCount);
    }

    // Handle disconnection
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"Disconnected: {connectionId}");

        // Remove from support staff if applicable
        _supportStaff.TryRemove(connectionId, out _);

        // Find and remove user connection if applicable
        var userIdToRemove = _userConnections
            .FirstOrDefault(x => x.Value == connectionId).Key;

        if (userIdToRemove != null)
        {
            Console.WriteLine($"Removing user connection: {userIdToRemove}");
            _userConnections.TryRemove(userIdToRemove, out _);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Get chat history for a specific user
    public async Task GetChatHistory(string userId)
    {
        Console.WriteLine($"GetChatHistory called for userId: {userId}");

        // KHÔNG trả về danh sách trống, mà trả về tin nhắn từ bộ nhớ
        // Vì chưa lưu tin nhắn ở server, hãy thông báo cho client lấy từ localStorage
        await Clients.Caller.SendAsync("LoadFromLocalStorage", userId);
    }
}