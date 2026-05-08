using System.Net;
using System.Net.Http.Json;
using BLL;
using BLL.ViewModels.Message;
using DAL.Extensions;
using Domain.Models.Contracts;
using Domain.Models.Freelance;
using Domain.Models.Messaging;
using Domain.Models.Projects;
using Domain.Models.Users;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using TestsData;

namespace Api.Tests.Integration;

public class MessageControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IAsyncLifetime
{
    private User _user = null!;
    private User _receiverUser = null!;
    private Contract _contract = null!;
    private Project _project = null!;
    private Freelancer _freelancer = null!;
    private Message _message = null!;

    [Fact]
    public async Task ShouldCreateMessage()
    {
        // Arrange
        var request = new CreateMessageVM
        {
            ContractId = _contract.Id,
            ReceiverEmail = _receiverUser.Email,
            Text = "Hello, this is a test message"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Message", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messageFromResponse = await JsonHelper.GetPayloadAsync<MessageVM>(response);
        var messageId = messageFromResponse.Id;

        var messageFromDb = await Context.Set<Message>().FirstOrDefaultAsync(x => x.Id == messageId);

        messageFromDb.Should().NotBeNull();
        messageFromDb.ContractId.Should().Be(_contract.Id);
        messageFromDb.ReceiverId.Should().Be(_receiverUser.Id);
        messageFromDb.Text.Should().Be("Hello, this is a test message");
        messageFromDb.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public async Task ShouldCreateMessageWithoutContract()
    {
        // Arrange
        var request = new CreateMessageWithoutContractVM
        {
            ReceiverEmail = _receiverUser.Email,
            Text = "Direct message without contract"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Message/without-contract", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messageFromResponse = await JsonHelper.GetPayloadAsync<MessageVM>(response);
        var messageId = messageFromResponse.Id;

        var messageFromDb = await Context.Set<Message>().FirstOrDefaultAsync(x => x.Id == messageId);

        messageFromDb.Should().NotBeNull();
        messageFromDb.ContractId.Should().BeNull();
        messageFromDb.ReceiverId.Should().Be(_receiverUser.Id);
        messageFromDb.Text.Should().Be("Direct message without contract");
        messageFromDb.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public async Task ShouldUpdateMessage()
    {
        // Arrange
        var request = new UpdateMessageVM
        {
            Text = "Updated message text"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Message/{_message.Id}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messageFromResponse = await JsonHelper.GetPayloadAsync<MessageVM>(response);

        var messageFromDb = await Context.Set<Message>().FirstOrDefaultAsync(x => x.Id == messageFromResponse.Id);

        messageFromDb.Should().NotBeNull();
        messageFromDb.Text.Should().Be("Updated message text");
    }

    [Fact]
    public async Task ShouldDeleteMessage()
    {
        // Act
        var response = await Client.DeleteAsync($"Message/{_message.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messageFromDb = await Context.Set<Message>().FirstOrDefaultAsync(x => x.Id == _message.Id);

        messageFromDb.Should().BeNull();
    }

    [Fact]
    public async Task ShouldGetMessageById()
    {
        // Act
        var response = await Client.GetAsync($"Message/{_message.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messageFromResponse = await JsonHelper.GetPayloadAsync<MessageVM>(response);

        messageFromResponse.Should().NotBeNull();
        messageFromResponse.Id.Should().Be(_message.Id);
        messageFromResponse.Text.Should().Be(_message.Text);
    }

    [Fact]
    public async Task ShouldGetMessagesByUser()
    {
        // Act
        var response = await Client.GetAsync("Message/by-user");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messages = await JsonHelper.GetPayloadAsync<List<MessageVM>>(response);

        messages.Should().NotBeEmpty();
        messages.Should().Contain(m => m.Id == _message.Id);
    }

    [Fact]
    public async Task ShouldGetMessagesByContract()
    {
        // Act
        var response = await Client.GetAsync($"Message/by-contract/{_contract.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messages = await JsonHelper.GetPayloadAsync<List<MessageVM>>(response);

        messages.Should().NotBeEmpty();
        messages.Should().Contain(m => m.Id == _message.Id);
    }

    [Fact]
    public async Task ShouldNotCreateMessageWithoutContractBecauseReceiverNotFound()
    {
        // Arrange
        var request = new CreateMessageWithoutContractVM
        {
            ReceiverEmail = "nonexistent@test.com",
            Text = "Test message"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Message/without-contract", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateMessageWithoutContractToYourself()
    {
        // Arrange
        var request = new CreateMessageWithoutContractVM
        {
            ReceiverEmail = _user.Email, // Sending to yourself
            Text = "Test message"
        };

        // Act
        var response = await Client.PostAsJsonAsync("Message/without-contract", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotCreateMessageWithEmptyText()
    {
        // Arrange
        var request = new CreateMessageWithoutContractVM
        {
            ReceiverEmail = _receiverUser.Email,
            Text = "" // Empty text
        };

        // Act
        var response = await Client.PostAsJsonAsync("Message/without-contract", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldNotUpdateBecauseNotFound()
    {
        // Arrange
        var request = new UpdateMessageVM
        {
            Text = "Updated text"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"Message/{Guid.NewGuid()}", request);

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotDeleteBecauseNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"Message/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldNotGetByIdBecauseNotFound()
    {
        // Act
        var response = await Client.GetAsync($"Message/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnEmptyListForContractWithNoMessages()
    {
        // Arrange
        var contractWithoutMessages = ContractData.CreateContract(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            createdById: _user.Id
        );
        await Context.AddAuditableAsync(contractWithoutMessages);
        await SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"Message/by-contract/{contractWithoutMessages.Id}");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();

        var messages = await JsonHelper.GetPayloadAsync<List<MessageVM>>(response);

        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldNotGetMessagesByContractBecauseContractNotFound()
    {
        // Act
        var response = await Client.GetAsync($"Message/by-contract/{Guid.NewGuid()}");

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task InitializeAsync()
    {
        _user = UserData.CreateTestUser(UserId, "sender@test.com");
        _receiverUser = UserData.CreateTestUser(email: "receiver@test.com",
            roleId: GetRoleIdByName(Settings.Roles.FreelancerRole));
        _project = ProjectData.CreateProject(userId: _user.Id);
        _freelancer = FreelancerData.CreateFreelancer(userId: _receiverUser.Id);
        _contract = ContractData.CreateContract(
            projectId: _project.Id,
            freelancerId: _freelancer.Id,
            createdById: _user.Id
        );
        _message = MessageData.CreateMessage(
            contractId: _contract.Id,
            receiverId: _receiverUser.Id,
            senderId: UserId,
            text: "Initial test message"
        );

        await Context.AddAuditableAsync(_user);
        await Context.AddAuditableAsync(_receiverUser);
        await Context.AddAuditableAsync(_project);
        await Context.AddAuditableAsync(_freelancer);
        await Context.AddAuditableAsync(_contract);
        await Context.AddAuditableAsync(_message);
        await SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearDatabaseAsync();
    }
}