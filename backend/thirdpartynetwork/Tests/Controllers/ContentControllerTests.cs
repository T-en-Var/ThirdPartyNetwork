// using System;
// using System.Collections.Generic;
// using AutoMapper;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using NUnit.Framework;
// using thirdpartynetwork.Core;
// using thirdpartynetwork.Core.Models;
// using thirdpartynetwork.Presentation.Controllers;
// using thirdpartynetwork.Application.Dtos.Content;
// using thirdpartynetwork.Application.Dtos.Network;
//
// namespace thirdpartynetwork.Tests.Controllers
// {
//     [TestFixture]
//     public class ContentControllerTests : ControllerTestBase
//     {
//         private Mock<IContentService> _mockContentService;
//         private ContentController _controller;
//         private Guid _networkId;
//         private Guid _contentId;
//         private Guid _createdByA;
//         private Guid _createdByB;
//         private Content _content;
//         private ContentDto _contentDto;
//         private List<Content> _contentList;
//         private List<ContentDto> _contentDtoList;
//         private CreateContentDto _createContentDto;
//         private UpdateContentDto _updateContentDto;
//
//         [SetUp]
//         public override void BaseSetup()
//         {
//             base.BaseSetup();
//             
//             _mockContentService = new Mock<IContentService>();
//             _controller = new ContentController(_mockContentService.Object, MockMapper.Object);
//             
//             _networkId = Guid.NewGuid();
//             _contentId = Guid.NewGuid();
//             _createdByA = Guid.NewGuid();
//             _createdByB = Guid.NewGuid();
//
//             Network network = new() { Name = "Test Network" };
//             
//             // Setup test data
//             _content = new Content
//             {
//                 Id = _contentId,
//                 Network = network,
//                 Value = "Test Content",
//                 CreatedBy = _createdByA
//             };
//             _contentDto = new ContentDto { 
//                 Id = _contentId, 
//                 Title = "Test Content", 
//                 Content = "Test content description", 
//                 Networks = new List<NetworkDto>() 
//             };
//             
//             _contentList = new List<Content>
//             {
//                 new Content { Id = Guid.NewGuid(), NetworkId = _networkId, Title = "Test Content 1" },
//                 new Content { Id = Guid.NewGuid(), NetworkId = _networkId, Title = "Test Content 2" }
//             };
//             
//             _contentDtoList = new List<ContentDto>
//             {
//                 new ContentDto { 
//                     Id = _contentList[0].Id, 
//                     Title = _contentList[0].Title, 
//                     Content = "Content description 1", 
//                     Networks = new List<NetworkDto>() 
//                 },
//                 new ContentDto { 
//                     Id = _contentList[1].Id, 
//                     Title = _contentList[1].Title, 
//                     Content = "Content description 2", 
//                     Networks = new List<NetworkDto>() 
//                 }
//             };
//             
//             _createContentDto = new CreateContentDto { Title = "New Content" };
//             _updateContentDto = new UpdateContentDto { Title = "Updated Content" };
//         }
//
//         #region GetNetworkContent Tests
//         
//         [Test]
//         public void GetNetworkContent_ReturnsOkResult_WithContentDtoList()
//         {
//             // Arrange
//             _mockContentService.Setup(service => service.GetByNetworkId(_networkId))
//                 .Returns(_contentList);
//                 
//             MockMapper.Setup(mapper => mapper.Map<IEnumerable<ContentDto>>(_contentList))
//                 .Returns(_contentDtoList);
//
//             // Act
//             var result = _controller.GetNetworkContent(_networkId);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<OkObjectResult>());
//             var okResult = (OkObjectResult)result;
//             
//             Assert.That(okResult.StatusCode, Is.EqualTo(200));
//             Assert.That(okResult.Value, Is.SameAs(_contentDtoList));
//             
//             _mockContentService.Verify(service => service.GetByNetworkId(_networkId), Times.Once);
//             MockMapper.Verify(mapper => mapper.Map<IEnumerable<ContentDto>>(_contentList), Times.Once);
//         }
//
//         [Test]
//         public void GetNetworkContent_ReturnsEmptyList_WhenNoContentExists()
//         {
//             // Arrange
//             var emptyList = new List<Content>();
//             var emptyDtoList = new List<ContentDto>();
//             
//             _mockContentService.Setup(service => service.GetByNetworkId(_networkId))
//                 .Returns(emptyList);
//                 
//             MockMapper.Setup(mapper => mapper.Map<IEnumerable<ContentDto>>(emptyList))
//                 .Returns(emptyDtoList);
//
//             // Act
//             var result = _controller.GetNetworkContent(_networkId);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<OkObjectResult>());
//             var okResult = (OkObjectResult)result;
//             
//             Assert.That(okResult.StatusCode, Is.EqualTo(200));
//             Assert.That(okResult.Value, Is.EquivalentTo(emptyDtoList));
//         }
//
//         [Test]
//         public void GetNetworkContent_HasCorrectPermissionAttribute()
//         {
//             VerifyRequiredPermission(typeof(ContentController), "GetNetworkContent", Constants.Permissions.ViewContent);
//         }
//         
//         #endregion
//
//         #region GetContent Tests
//         
//         [Test]
//         public void GetContent_ReturnsOkResult_WithContentDto_WhenContentExists()
//         {
//             // Arrange
//             _mockContentService.Setup(service => service.GetById(_contentId))
//                 .Returns(_content);
//                 
//             MockMapper.Setup(mapper => mapper.Map<ContentDto>(_content))
//                 .Returns(_contentDto);
//
//             // Act
//             var result = _controller.GetContent(_networkId, _contentId);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<OkObjectResult>());
//             var okResult = (OkObjectResult)result;
//             
//             Assert.That(okResult.StatusCode, Is.EqualTo(200));
//             Assert.That(okResult.Value, Is.SameAs(_contentDto));
//             
//             _mockContentService.Verify(service => service.GetById(_contentId), Times.Once);
//             MockMapper.Verify(mapper => mapper.Map<ContentDto>(_content), Times.Once);
//         }
//
//         [Test]
//         public void GetContent_ReturnsNotFound_WhenContentDoesNotExist()
//         {
//             // Arrange
//             _mockContentService.Setup(service => service.GetById(_contentId))
//                 .Returns((Content)null);
//
//             // Act
//             var result = _controller.GetContent(_networkId, _contentId);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<NotFoundResult>());
//             
//             _mockContentService.Verify(service => service.GetById(_contentId), Times.Once);
//             MockMapper.Verify(mapper => mapper.Map<ContentDto>(It.IsAny<Content>()), Times.Never);
//         }
//
//         [Test]
//         public void GetContent_HasCorrectPermissionAttribute()
//         {
//             VerifyRequiredPermission(typeof(ContentController), "GetContent", Constants.Permissions.ViewContent);
//         }
//         
//         #endregion
//
//         #region CreateContent Tests
//         
//         [Test]
//         public void CreateContent_ReturnsOkResult_WithCreatedContentDto()
//         {
//             // Arrange
//             _mockContentService.Setup(service => service.Create(_networkId, _createContentDto))
//                 .Returns(_content);
//                 
//             MockMapper.Setup(mapper => mapper.Map<ContentDto>(_content))
//                 .Returns(_contentDto);
//
//             // Act
//             var result = _controller.CreateContent(_networkId, _createContentDto);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<OkObjectResult>());
//             var okResult = (OkObjectResult)result;
//             
//             Assert.That(okResult.StatusCode, Is.EqualTo(200));
//             Assert.That(okResult.Value, Is.SameAs(_contentDto));
//             
//             _mockContentService.Verify(service => service.Create(_networkId, _createContentDto), Times.Once);
//             MockMapper.Verify(mapper => mapper.Map<ContentDto>(_content), Times.Once);
//         }
//
//         [Test]
//         public void CreateContent_HasCorrectPermissionAttribute()
//         {
//             VerifyRequiredPermission(typeof(ContentController), "CreateContent", Constants.Permissions.CreateContent);
//         }
//         
//         #endregion
//
//         #region UpdateContent Tests
//         
//         [Test]
//         public void UpdateContent_ReturnsOkResult_WithUpdatedContentDto()
//         {
//             // Arrange
//             _mockContentService.Setup(service => service.Update(_networkId, _contentId, _updateContentDto))
//                 .Returns(_content);
//                 
//             MockMapper.Setup(mapper => mapper.Map<ContentDto>(_content))
//                 .Returns(_contentDto);
//
//             // Act
//             var result = _controller.UpdateContent(_networkId, _contentId, _updateContentDto);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<OkObjectResult>());
//             var okResult = (OkObjectResult)result;
//             
//             Assert.That(okResult.StatusCode, Is.EqualTo(200));
//             Assert.That(okResult.Value, Is.SameAs(_contentDto));
//             
//             _mockContentService.Verify(service => service.Update(_networkId, _contentId, _updateContentDto), Times.Once);
//             MockMapper.Verify(mapper => mapper.Map<ContentDto>(_content), Times.Once);
//         }
//
//         [Test]
//         public void UpdateContent_HasCorrectPermissionAttribute()
//         {
//             VerifyRequiredPermission(typeof(ContentController), "UpdateContent", Constants.Permissions.EditContent);
//         }
//         
//         #endregion
//
//         #region DeleteContent Tests
//         
//         [Test]
//         public void DeleteContent_ReturnsNoContent()
//         {
//             // Arrange
//             _mockContentService.Setup(service => service.DeleteById(_contentId))
//                 .Verifiable();
//
//             // Act
//             var result = _controller.DeleteContent(_networkId, _contentId);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<NoContentResult>());
//             
//             _mockContentService.Verify(service => service.DeleteById(_contentId), Times.Once);
//         }
//
//         [Test]
//         public void DeleteContent_HasCorrectPermissionAttribute()
//         {
//             VerifyRequiredPermission(typeof(ContentController), "DeleteContent", Constants.Permissions.DeleteContent);
//         }
//         
//         #endregion
//     }
// }