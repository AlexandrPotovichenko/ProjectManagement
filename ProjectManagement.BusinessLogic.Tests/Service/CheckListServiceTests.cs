using Moq;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.BusinessLogic.Services.Implementation;
using ProjectManagement.BusinessLogic.Services.Interfaces;
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
    public class CheckListServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICardRepository> _cardRepositoryMock;
        private readonly Mock<ICheckListRepository> _checkListRepositoryMock;
        private readonly Mock<IUserManager> _userManagerMock;
        private readonly CheckListService _service;
        private int _currentUserId = 1;
        public CheckListServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            _cardRepositoryMock = new Mock<ICardRepository>();
            _cardRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _checkListRepositoryMock = new Mock<ICheckListRepository>();
            _checkListRepositoryMock.Setup(x => x.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);

            _userManagerMock = new Mock<IUserManager>();
            _userManagerMock.Setup(x => x.GetCurrentUserId()).Returns(_currentUserId);
            _service = new CheckListService(_checkListRepositoryMock.Object,_cardRepositoryMock.Object, _userManagerMock.Object);
        }     
        [Fact]
        public async void CreateCheckListAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            string checkListName = "Check List 1";
            Card card = GetCard();
            _cardRepositoryMock.Setup(repo => repo.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            // Act
            CheckList checkList = await _service.CreateCheckListAsync(cardId, checkListName);
            // Assert
            Assert.NotNull(checkList);
            Assert.Contains(card.CheckLists, cl=>cl.Name==checkListName);
        }
        [Fact]
        public async void CreateCheckListAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            string checkListName = "Check List 1";
            Card card = GetCard();
            _cardRepositoryMock.Setup(repo => repo.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CreateCheckListAsync(wrongCardId, checkListName));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void CreateCheckListAsync_MemberIsObserver_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            string checkListName = "Check List 1";
            Card card = GetCard();
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            _cardRepositoryMock.Setup(repo => repo.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.Id == cardId))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CreateCheckListAsync(cardId, checkListName));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void AddCheckListItemToCheckListAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) {Id = checkListId, Card = card,CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            // Act
            CheckListItem newCheckListItem = await _service.AddCheckListItemToCheckListAsync(checkListId, checkListName);
            // Assert
            Assert.NotNull(newCheckListItem);
            Assert.Equal(checkListName, newCheckListItem.Name);
        }
        [Fact]
        public async void AddCheckListItemToCheckListAsync_NotExistingCheckListId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int wrongCheckListId = 2;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddCheckListItemToCheckListAsync(wrongCheckListId, checkListName));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void AddCheckListItemToCheckListAsync_MemberIsObserver_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.AddCheckListItemToCheckListAsync(checkListId, checkListName));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void GetCheckListsByCardIdAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            // Act
            IEnumerable<CheckList> checkLists = await _service.GetCheckListsByCardIdAsync(cardId);
            // Assert
            Assert.Collection(checkLists,item => Assert.Equal(checkListId, item.Id));
            Assert.Single(checkLists);
        }
        [Fact]
        public async void GetCheckListsByCardIdAsync_NotExistingCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int wrongCardId = 2;
            int checkListId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCheckListsByCardIdAsync(wrongCardId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void GetCheckListsByCardIdAsync_UserIsNotMemberOnCardId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            card.CardMembers = new List<CardMember>();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCheckListsByCardIdAsync(cardId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void GetCheckListItemsAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            checkList.ChecklistItems.Add(new CheckListItem(checkListItemName) { Id = checkListItemId });
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            // Act
            IEnumerable<CheckListItem> checkListItems = await _service.GetCheckListItemsAsync(checkListId);
            // Assert
            Assert.Collection(checkListItems, item => Assert.Equal(checkListId, item.Id));
            Assert.Single(checkListItems);
        }
        [Fact]
        public async void GetCheckListItemsAsync_NotExistingCheckListId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int wrongCheckListId = 2;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            checkList.ChecklistItems.Add(new CheckListItem(checkListItemName) { Id = checkListItemId });
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCheckListsByCardIdAsync(wrongCheckListId));
            _checkListRepositoryMock.Verify(service => service.GetWithItemsAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void GetCheckListItemsAsync_UserIsNotMemberOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;         
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            card.CardMembers = new List<CardMember>();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            checkList.ChecklistItems.Add(new CheckListItem(checkListItemName) { Id = checkListItemId });
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.GetCheckListsByCardIdAsync(checkListId));
            _checkListRepositoryMock.Verify(service => service.GetWithItemsAsync(It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async void CompleteCheckListItemAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            // Act
            await _service.CompleteCheckListItemAsync(checkListId,checkListItemId);
            // Assert
            _cardRepositoryMock.Verify(x => x.UpdateAsync(
                It.Is<Card>(item => item.Id == cardId)), Times.Once);
            Assert.True(checkListItem.IsDone);
        }
        [Fact]
        public async void CompleteCheckListItemAsync_NotExistingCheckListId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int wrongCheckListId = 2;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CompleteCheckListItemAsync(wrongCheckListId, checkListItemId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never); 
        }
        [Fact]
        public async void CompleteCheckListItemAsync_NotExistingCheckListItemId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            int wrongCheckListIdItemId = 2;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CompleteCheckListItemAsync(checkListId, wrongCheckListIdItemId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void CompleteCheckListItemAsync_UserIsNotMemberOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            card.CardMembers = new List<CardMember>();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.CompleteCheckListItemAsync(checkListId, checkListItemId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCheckListAsync_ExistingId_Passed()
        {
            //Arrange
            int checkListId = 1;
            int cardId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            // Act
            await _service.DeleteCheckListAsync(checkListId);
            // Assert
            _cardRepositoryMock.Verify(x => x.UpdateAsync(
                It.Is<Card>(item => item.Id == cardId)), Times.Once);
            Assert.DoesNotContain(card.CheckLists,cl=>cl.Id == checkListId);
        }
        [Fact]
        public async void DeleteCheckListAsync_NotExistingCheckListId_ExceptionIsThrown()
        {
            //Arrange
            int checkListId = 1;
            int wrongCheckListId = 2;
            int cardId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListAsync(wrongCheckListId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCheckListAsync_UserIsNotMemberOnCard_ExceptionIsThrown()
        {
            //Arrange
            int checkListId = 1;
            int cardId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            card.CardMembers = new List<CardMember>();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListAsync(checkListId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCheckListAsync_MemberIsObserver_ExceptionIsThrown()
        {
            //Arrange
            int checkListId = 1;
            int cardId = 1;
            string checkListName = "CheckList 1";
            Card card = GetCard();
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListAsync(checkListId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCheckListItemAsync_ExistingId_Passed()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            // Act
            await _service.DeleteCheckListItemAsync(checkListId, checkListItemId);
            // Assert
            _cardRepositoryMock.Verify(x => x.UpdateAsync(
                It.Is<Card>(item => item.Id == cardId)), Times.Once);
            Assert.DoesNotContain(checkList.ChecklistItems, cli => cli.Id == checkListItemId);
        }
        [Fact]
        public async void DeleteCheckListItemAsync_NotExistingCheckListId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int wrongCheckListId = 2;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListItemAsync(wrongCheckListId, checkListItemId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCheckListItemAsync_NotExistingCheckListItemId_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int wrongCheckListItemId = 2;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListItemAsync(checkListId, wrongCheckListItemId));
        }
        [Fact]
        public async void DeleteCheckListItemAsync_UserIsNotMemberOnCard_ExceptionIsThrown()
        {
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            card.CardMembers = new List<CardMember>();
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListItemAsync(checkListId, checkListItemId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        [Fact]
        public async void DeleteCheckListItemAsync_MemberIsObserver_ExceptionIsThrown()
        { 
            //Arrange
            int cardId = 1;
            int checkListId = 1;
            int checkListItemId = 1;
            string checkListName = "CheckList 1";
            string checkListItemName = "CheckListItem 1";
            Card card = GetCard();
            CardMember currentCardMember = card.CardMembers.FirstOrDefault(bm => bm.UserId == _currentUserId);
            currentCardMember.Role = Role.Observer;
            CheckList checkList = new CheckList(checkListName) { Id = checkListId, Card = card, CardId = cardId };
            CheckListItem checkListItem = new CheckListItem(checkListItemName) { Id = checkListItemId };
            checkList.ChecklistItems.Add(checkListItem);
            card.CheckLists.Add(checkList);
            _checkListRepositoryMock.Setup(x => x.GetWithItemsAsync(checkListId)).Returns(Task.FromResult(checkList));
            _cardRepositoryMock.Setup(x => x.GetWithItemsByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.GetForEditByIdAsync(cardId)).Returns(Task.FromResult(card));
            _cardRepositoryMock.Setup(x => x.UpdateAsync(It.Is<Card>(b => b.CheckLists.Any(cl => cl.Id == checkListId)))).Returns(Task.CompletedTask);
            //Act
            //Assert
            await Assert.ThrowsAsync<WebAppException>(async () => await _service.DeleteCheckListItemAsync(checkListId, checkListItemId));
            _cardRepositoryMock.Verify(service => service.UpdateAsync(It.IsAny<Card>()), Times.Never);
        }
        private Card GetCard()
        {
            int cardId = 1;
            List<CardMember> cardMembers = new List<CardMember>();
            CardMember cardMember = new CardMember(1, Role.Admin) { Id = 1, CardId = cardId };
            cardMembers.Add(cardMember);
            Card card = new Card
            {
                Id = cardId,
                Name = "Card 1",
                CardMembers = cardMembers
            };
            return card;
        }
    }
}
