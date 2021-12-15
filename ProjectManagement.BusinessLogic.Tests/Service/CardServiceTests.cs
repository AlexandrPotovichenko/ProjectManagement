using Moq;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Implementation;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.BusinessLogic.Specifications;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ProjectManagement.BusinessLogic.Tests.Service
{
    public class CardServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<ICardMemberRepository> _cardMemberRepositoryMock;
        private readonly Mock<IListRepository> _listRepositoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly CardService _service;

        private int _currentUserId = 1;
        public CardServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            _cardRepositoryMock = new Mock<ICardRepository>();
            _cardRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _cardMemberRepositoryMock = new Mock<ICardMemberRepository>();
            _cardMemberRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _listRepositoryMock = new Mock<IListRepository>();
            _listRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _userManagerMock = new Mock<IUserManager>();
            _userManagerMock.Setup(x => x.GetCurrentUserId()).Returns(_currentUserId);

            _service = new CardService(_cardRepositoryMock.Object, _listRepositoryMock.Object, _cardMemberRepositoryMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async void GetCardsAsync_CardsExist_Passed()
        {
            //Arrange
            _cardRepositoryMock.Setup(x => x.GetWithItemsAsync(_currentUserId))
                            .Returns(Task.FromResult(GetCards().AsEnumerable()));
            //Act
            IEnumerable<Card> cards = await _service.GetCardsAsync();
            //Assert
            Assert.Equal(3, cards.Count());
        }
        [Fact]
        public async void GetCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId)))
                .Returns(Task.FromResult(cardMember));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            Card cardResult = await _service.GetCardAsync(cardId);
            //Assert
            Assert.Equal(card, cardResult);
        }
        [Fact]
        public async void GetCardAsync_CardNotExist_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId)))
                .Returns(Task.FromResult(cardMember));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCardAsync(wrongCardId));
            _cardRepositoryMock.Verify(service => service.GetWithItemsByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetCardAsync_UserIsNotAnMemberOfCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int cardMemberUserId = 2;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(cardMemberUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == cardMemberUserId)))
                .Returns(Task.FromResult(cardMember));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCardAsync(cardId));
            _cardRepositoryMock.Verify(service => service.GetWithItemsByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void CreateCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardMemberRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<CardMember>())).Returns(Task.FromResult(cardMember));
            _cardRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<Card>())).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(repo => repo.CanCreateCardAsync(boardId, _currentUserId)).Returns(Task.FromResult(true));
            _listRepositoryMock.Setup(repo => repo.GetForEditByIdAsync(listId)).Returns(Task.FromResult(list));
            // Act
            Card cardResult = await _service.CreateCardAsync(cardName, description, listId);
            // Assert
            Assert.NotNull(cardResult);
            Assert.Equal(cardId, cardResult.Id);
        }
        [Fact]
        public async void CreateCardAsync_ListNotExist_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int listId = 1;
            int wrongListId = 2;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<Card>())).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(repo => repo.CanCreateCardAsync(boardId, _currentUserId)).Returns(Task.FromResult(true));
            _listRepositoryMock.Setup(repo => repo.GetForEditByIdAsync(listId)).Returns(Task.FromResult(list));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CreateCardAsync(cardName, description, wrongListId));
            _cardRepositoryMock.Verify(service => service.InsertAsync(It.IsAny<Card>()), Times.Never);
        }

        [Fact]
        public async void CreateCardAsync_UserObserverOnBoard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Observer);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<Card>())).Returns(Task.FromResult(card));
            // return the false, because the user is Observer on the board
            _cardRepositoryMock.Setup(repo => repo.CanCreateCardAsync(boardId, _currentUserId)).Returns(Task.FromResult(false));
            _listRepositoryMock.Setup(repo => repo.GetForEditByIdAsync(listId)).Returns(Task.FromResult(list));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CreateCardAsync(cardName, description, listId));
            _cardRepositoryMock.Verify(service => service.InsertAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId)))
                .Returns(Task.FromResult(cardMember));
            _cardRepositoryMock.Setup(repo => repo.DeleteByIdAsync(boardId)).Returns(Task.CompletedTask);
            // Act
            await _service.DeleteCardAsync(cardId);
            // Assert
            _cardRepositoryMock.Verify(repo => repo.DeleteByIdAsync(cardId), Times.Once);
        }
        [Fact]
        public async void DeleteCardAsync_NotExistingId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            int listId = 1;
            int boardId = 1;
            string cardName = "Card 1";
            string description = "Description Card 1";
            List list = new List() { Id = listId, BoardId = boardId };
            CardMember cardMember = new CardMember(_currentUserId, Role.Admin);
            Card card = new Card(cardName, description, cardMember, listId) { Id = cardId };
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId)))
                .Returns(Task.FromResult(cardMember));
            _cardRepositoryMock.Setup(repo => repo.DeleteByIdAsync(boardId)).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCardAsync(wrongCardId));
            _cardRepositoryMock.Verify(service => service.DeleteByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetAllCardMembersAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId))).Returns(Task.FromResult(cardMember));
            // Act
            IEnumerable<CardMember> members = await _service.GetAllCardMembersAsync(cardId);
            // Assert
            Assert.Collection(members,
                item => Assert.Equal(1, item.UserId),
                item => Assert.Equal(2, item.UserId),
                item => Assert.Equal(3, item.UserId)
            );
            Assert.Equal(members.Count(), card.CardMembers.Count);
        }
        [Fact]
        public async void GetAllCardMembersAsync_NotExistingId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId))).Returns(Task.FromResult(cardMember));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetAllCardMembersAsync(wrongCardId));
            _cardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetAllCardMembersAsync_UserIsNotCardMember_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1; 
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            card.CardMembers.Remove(cardMember);
            _cardRepositoryMock.Setup(x => x.GetByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetAllCardMembersAsync(cardId));
            _cardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void AddMemberToCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int newMemberUserId = 4;
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            User user = new User("UserNeme", "PassHash") { Id = newMemberUserId };
            Role role = Role.Normal;
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _userManagerMock.Setup(x => x.GetUserByIdAsync(newMemberUserId)).Returns(Task.FromResult(user));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _userManagerMock.Setup(x => x.IsUserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            // Act
            CardMember cardMember = await _service.AddMemberToCardAsync(newMemberUserId, cardId, role);
            // Assert
            Assert.NotNull(cardMember);
            Assert.Equal(newMemberUserId, cardMember.UserId);
        }
        [Fact]
        public async void AddMemberToCardAsync_NotExistingUserId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int newMemberUserId = 4;
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            User user = new User("UserNeme", "PassHash") { Id = newMemberUserId };
            Role role = Role.Normal;
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _userManagerMock.Setup(x => x.IsUserExistsAsync(newMemberUserId)).Returns(Task.FromResult(false));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddMemberToCardAsync(newMemberUserId, cardId, role));
            _cardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void AddMemberToCardAsync_CurrentMemberIsNotAdminOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int newMemberUserId = 4;
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            currentMember.Role = Role.Observer;
            User user = new User("UserNeme", "PassHash") { Id = newMemberUserId };
            Role role = Role.Normal;
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _userManagerMock.Setup(x => x.GetUserByIdAsync(newMemberUserId)).Returns(Task.FromResult(user));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _userManagerMock.Setup(x => x.IsUserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddMemberToCardAsync(newMemberUserId, cardId, role));
            _cardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void AddMemberToCardAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            int newMemberUserId = 4;
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            User user = new User("UserNeme", "PassHash") { Id = newMemberUserId };
            Role role = Role.Normal;
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _userManagerMock.Setup(x => x.GetUserByIdAsync(newMemberUserId)).Returns(Task.FromResult(user));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _userManagerMock.Setup(x => x.IsUserExistsAsync(newMemberUserId)).Returns(Task.FromResult(true));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CardMembers.Any(bm => bm.UserId == newMemberUserId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddMemberToCardAsync(newMemberUserId, wrongCardId, role));
            _cardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void RemoveMemberFromCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int memberIdForDelete = 2;
            User userForDelete = new User("UserNeme", "PassHash") { Id = memberIdForDelete };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMemberForDelete = card.CardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetWithMembersAsync(cardId)).Returns(Task.FromResult(card));
            _userManagerMock.Setup(x => x.GetUserByIdAsync(memberIdForDelete)).Returns(Task.FromResult(userForDelete));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            // Act
            await _service.RemoveMemberFromCardAsync(memberIdForDelete, cardId);
            // Assert
            Assert.DoesNotContain(card.CardMembers, b => b.Id == memberIdForDelete);
        }
        [Fact]
        public async void RemoveMemberFromCardAsync_NotExistingMemberId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int memberIdForDelete = 2;
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMemberForDelete = card.CardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetWithMembersAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.RemoveMemberFromCardAsync(memberIdForDelete, cardId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void RemoveMemberFromCardAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            string cardName = "Card 1";
            int memberIdForDelete = 2;
            User userForDelete = new User("UserNeme", "PassHash") { Id = memberIdForDelete };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMemberForDelete = card.CardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetWithMembersAsync(cardId)).Returns(Task.FromResult(card));
            _userManagerMock.Setup(x => x.GetUserByIdAsync(memberIdForDelete)).Returns(Task.FromResult(userForDelete));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.RemoveMemberFromCardAsync(memberIdForDelete, wrongCardId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void RemoveMemberFromCardAsync_CurrentMemberIsNotAdminOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int memberIdForDelete = 2;
            User userForDelete = new User("UserNeme", "PassHash") { Id = memberIdForDelete };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember cardMemberForDelete = card.CardMembers.FirstOrDefault(bm => bm.Id == memberIdForDelete);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            _cardRepositoryMock.Setup(x => x.GetWithMembersAsync(cardId)).Returns(Task.FromResult(card));
            _userManagerMock.Setup(x => x.GetUserByIdAsync(memberIdForDelete)).Returns(Task.FromResult(userForDelete));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.RemoveMemberFromCardAsync(memberIdForDelete, cardId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int memberIdForUpdate = 2;
            int userIdForUpdate = 2;
            string cardName = "card 1";
            User userForUpdate = new User("UserNeme", "PassHash") { Id = userIdForUpdate };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _userManagerMock.Setup(x => x.GetUserByIdAsync(userIdForUpdate)).Returns(Task.FromResult(userForUpdate));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            // Act
            await _service.UpdateMembershipOfMemberOnCardAsync(cardId, memberIdForUpdate, Role.Observer);
            // Assert
            Assert.Equal(Role.Observer, card.CardMembers.FirstOrDefault(bm => bm.Id == memberIdForUpdate)?.Role);
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnCardAsync_NotExistingMemberId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongMemberIdForUpdate = 4;
            int userIdForUpdate = 2;
            string cardName = "card 1";
            User userForUpdate = new User("UserNeme", "PassHash") { Id = userIdForUpdate };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _userManagerMock.Setup(x => x.GetUserByIdAsync(userIdForUpdate)).Returns(Task.FromResult(userForUpdate));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.UpdateMembershipOfMemberOnCardAsync(cardId, wrongMemberIdForUpdate, Role.Observer));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnCardAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            int memberIdForUpdate = 2;
            int userIdForUpdate = 2;
            string cardName = "card 1";
            User userForUpdate = new User("UserNeme", "PassHash") { Id = userIdForUpdate };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _userManagerMock.Setup(x => x.GetUserByIdAsync(userIdForUpdate)).Returns(Task.FromResult(userForUpdate));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.UpdateMembershipOfMemberOnCardAsync(wrongCardId, memberIdForUpdate, Role.Observer));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void UpdateMembershipOfMemberOnCardAsync_CurrentMemberIsNotAdminOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int memberIdForUpdate = 2;
            int userIdForUpdate = 2;
            string cardName = "card 1";
            User userForUpdate = new User("UserNeme", "PassHash") { Id = userIdForUpdate };
            Card card = GetCardWithMembers(cardId, cardName);
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            _userManagerMock.Setup(x => x.GetUserByIdAsync(userIdForUpdate)).Returns(Task.FromResult(userForUpdate));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.UpdateMembershipOfMemberOnCardAsync(cardId, memberIdForUpdate, Role.Observer));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void AddCommentToCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int cardMemberId = 1;
            int commentId = 1;
            string comment = "Comment on Card 1";
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, comment, true) { Id = commentId });
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Actions.Any(a => a.IsComment && a.Description == comment)))).Returns(Task.FromResult(card));
            // Act
            CardAction commentResult = await _service.AddCommentToCardAsync(cardId, comment);
            // Assert
            Assert.NotNull(commentResult);
            Assert.True(commentResult.IsComment);
        }
        [Fact]
        public async void AddCommentToCardAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            int cardMemberId = 1;
            int commentId = 1;
            string comment = "Comment on Card 1";
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, comment, true) { Id = commentId });
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Actions.Any(a => a.IsComment && a.Description == comment)))).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddCommentToCardAsync(wrongCardId, comment));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void AddCommentToCardAsync_UserObserverOnBoard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int cardMemberId = 1;
            int commentId = 1;
            string comment = "Comment on Card 1";
            string cardName = "Card 1";
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, comment, true) { Id = commentId });
            CardMember currentMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            currentMember.Role = Role.Observer;
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms =>
            ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentMember));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Actions.Any(a => a.IsComment && a.Description == comment)))).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddCommentToCardAsync(cardId, comment));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void GetCommentsOnCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int cardMemberId = 1;
            string description = "Description Card";
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember cardMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId)))
                .Returns(Task.FromResult(cardMember));
            // Act
            IEnumerable<CardAction> cardActions = await _service.GetCommentsOnCardAsync(cardId);
            // Assert
            Assert.Collection(cardActions,
                item => Assert.Equal(1, item.Id),
                item => Assert.Equal(2, item.Id),
                item => Assert.Equal(3, item.Id)
            );
            Assert.Equal(cardActions.Count(), card.Actions.Count);
        }
        [Fact]
        public async void GetCommentsOnCardAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            string cardName = "Card 1";
            int cardMemberId = 1;
            string description = "Description Card";
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember cardMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            _cardRepositoryMock.Setup(x => x.GetByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.CardId == cardId && ms.UserId == _currentUserId)))
                .Returns(Task.FromResult(cardMember));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCommentsOnCardAsync(wrongCardId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void GetCommentsOnCardAsync_UserNotMemberOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int cardMemberId = 1;
            string description = "Description Card";
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember cardMember = card.CardMembers.FirstOrDefault(m => m.UserId == _currentUserId);
            card.CardMembers.Remove(cardMember);
            _cardRepositoryMock.Setup(x => x.GetByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCommentsOnCardAsync(cardId));
            _cardRepositoryMock.Verify(service => service.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void DeleteCommentOnCardAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int cardMemberId = 1;
            string description = "Description Card";
            int commentId = 2;
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            // Act
            await _service.DeleteCommentOnCardAsync(cardId, commentId);
            // Assert
            Assert.DoesNotContain(card.Actions, b => b.Id == commentId);
        }
        [Fact]
        public async void DeleteCommentOnCardAsync_NotExistingCardId_ExceptionIsThrown() 
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            string cardName = "Card 1";
            int cardMemberId = 1;
            string description = "Description Card";
            int commentId = 2;
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCommentOnCardAsync(wrongCardId, commentId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCommentOnCardAsync_NotExistingCommentId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int cardMemberId = 1;
            string description = "Description Card";
            int wrongCommentId = 4;
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCommentOnCardAsync(cardId, wrongCommentId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCommentOnCardAsync_MemberIsNotTheAuthorOfTheComment_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            int cardMemberId = 2;
            string description = "Description Card";
            int commentId = 1;
            Card card = GetCardWithMembers(cardId, cardName);
            card.Actions.Add(new CardAction(cardMemberId, description + " 1", true) { Id = 1 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 2", true) { Id = 2 });
            card.Actions.Add(new CardAction(cardMemberId, description + " 3", true) { Id = 3 });
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCommentOnCardAsync(cardId, commentId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void MoveCardToListAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            List oldList = new List() { Id = 1, BoardId = 1, Name = "List 1" };
            List newList = new List() { Id = 2, BoardId = 1, Name = "List 2" };
            Card card = GetCardWithMembers(cardId, cardName);
            card.List = oldList;
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _listRepositoryMock.Setup(x => x.GetByIdAsync(oldList.Id)).Returns(Task.FromResult(oldList));
            _listRepositoryMock.Setup(x => x.GetByIdAsync(newList.Id)).Returns(Task.FromResult(newList));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            // Act
            await _service.MoveCardToListAsync(cardId, newList.Id);
            // Assert
            Assert.Equal(card.List, newList);
        }
        [Fact]
        public async void MoveCardToListAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            string cardName = "Card 1";
            List oldList = new List() { Id = 1, BoardId = 1, Name = "List 1" };
            List newList = new List() { Id = 2, BoardId = 1, Name = "List 2" };
            Card card = GetCardWithMembers(cardId, cardName);
            card.List = oldList;
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _listRepositoryMock.Setup(x => x.GetByIdAsync(oldList.Id)).Returns(Task.FromResult(oldList));
            _listRepositoryMock.Setup(x => x.GetByIdAsync(newList.Id)).Returns(Task.FromResult(newList));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCommentOnCardAsync(wrongCardId, newList.Id));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void MoveCardToListAsync_NotExistingListId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            List oldList = new List() { Id = 1, BoardId = 1, Name = "List 1" };
            int wrongNewListId = 2;
            Card card = GetCardWithMembers(cardId, cardName);
            card.List = oldList;
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _listRepositoryMock.Setup(x => x.GetByIdAsync(oldList.Id)).Returns(Task.FromResult(oldList));
           // _listRepositoryMock.Setup(x => x.GetByIdAsync(newList.Id)).Returns(Task.FromResult(newList));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCommentOnCardAsync(cardId, wrongNewListId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void MoveCardToListAsync_MemberIsObserver_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string cardName = "Card 1";
            List oldList = new List() { Id = 1, BoardId = 1, Name = "List 1" };
            List newList = new List() { Id = 2, BoardId = 1, Name = "List 2" };
            Card card = GetCardWithMembers(cardId, cardName);
            card.List = oldList;
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            _cardMemberRepositoryMock.Setup(x => x.GetSingleAsync(It.Is<GetCardMemberByUserIdSpecification>(ms => ms.UserId == _currentUserId && ms.CardId == cardId)))
                .Returns(Task.FromResult(currentCardMember));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _listRepositoryMock.Setup(x => x.GetByIdAsync(oldList.Id)).Returns(Task.FromResult(oldList));
             _listRepositoryMock.Setup(x => x.GetByIdAsync(newList.Id)).Returns(Task.FromResult(newList));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCommentOnCardAsync(cardId, newList.Id));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        private List<Card> GetCards()
        {
            List<Card> cards = new List<Card>();
            cards.Add(new Card() { Id = 1, Name = "Card 1" });
            cards.Add(new Card() { Id = 2, Name = "Card 2" });
            cards.Add(new Card() { Id = 3, Name = "Card 3" });
            return cards;
        }
        private Card GetCardWithMembers(int cardId, string cardName)
        {
            CardMember cardMember;
            List<CardMember> cardMembers = new List<CardMember>();
            cardMember = new CardMember(1, Role.Admin) { Id = 1, CardId = cardId };
            cardMembers.Add(cardMember);
            cardMember = new CardMember(2, Role.Normal) { Id = 2, CardId = cardId };
            cardMembers.Add(cardMember);
            cardMember = new CardMember(3, Role.Observer) { Id = 3, CardId = cardId };
            cardMembers.Add(cardMember);
            Card card = new Card
            {
                Id = cardId,
                Name = cardName,
                CardMembers = cardMembers
            };
            return card;
        }
    }
}
