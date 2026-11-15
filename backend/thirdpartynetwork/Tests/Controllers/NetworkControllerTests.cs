// using AutoMapper;
// using Moq;
// using NUnit.Framework;
// using Microsoft.AspNetCore.Mvc;
// using thirdpartynetwork.Core.Models;
// using thirdpartynetwork.Presentation.Controllers;
//
// namespace thirdpartynetwork.Tests.Controllers
// {
//     [TestFixture]
//     public class NetworkControllerTests
//     {
//         private Mock<INetworkService> _networkServiceMock;
//         private Mock<IMapper> _mapperMock;
//         private NetworkController _controller;
//         private List<Network> _networks;
//         private List<NetworkDto> _networkDtos;
//
//         [SetUp]
//         public void Setup()
//         {
//             _networkServiceMock = new Mock<INetworkService>();
//             _mapperMock = new Mock<IMapper>();
//
//             _networks = new List<Network>
//             {
//                 new() { Id = Guid.NewGuid(), Name = "TestNetwork1", Description = "", ImageUrl = "", IsPublic = true },
//                 new() { Id = Guid.NewGuid(), Name = "TestNetwork2", Description = "", ImageUrl = "", IsPublic = false }
//             };
//
//             _networkDtos = new List<NetworkDto>
//             {
//                 new() { Id = _networks[0].Id, Name = "TestNetwork1", Description = "", IsSystemProtected = true, IsPublic = true },
//                 new() { Id = _networks[1].Id, Name = "TestNetwork2", Description = "", IsSystemProtected = false, IsPublic = false }
//             };
//
//             _networkServiceMock.Setup(s => s.GetAll()).Returns(_networks);
//             _mapperMock.Setup(m => m.Map<IEnumerable<NetworkDto>>(_networks)).Returns(_networkDtos);
//
//             _controller = new NetworkController(_networkServiceMock.Object, _mapperMock.Object);
//         }
//
//         [Test]
//         public void GetNetworks_ShouldReturnListOfNetworks()
//         {
//             // Act
//             var result = _controller.GetNetworks() as ObjectResult;
//
//             // Assert
//             Assert.That(result, Is.Not.Null);
//             Assert.That(result!.StatusCode, Is.EqualTo(200));
//             Assert.That(result.Value, Is.EqualTo(_networkDtos));
//         }
//
//         [Test]
//         public void GetNetworkById_ShouldReturnNetwork_WhenNetworkExists()
//         {
//             // Act
//             var result = _controller.GetNetworkById(_networks[0].Id) as OkObjectResult;
//
//             // Assert
//             Assert.That(result, Is.Not.Null);
//             Assert.That(result!.StatusCode, Is.EqualTo(200));
//             Assert.That(result.Value, Is.EqualTo(_networkDtos[0]));
//         }
//
//         [Test]
//         public void GetNetworkById_ShouldReturnNotFound_WhenNetworkDoesNotExist()
//         {
//             // Arrange
//             var networkId = Guid.NewGuid();
//
//             // _networkServiceMock.Setup(s => s.GetById(networkId)).Returns((Network)null!);
//
//             // Act
//             var result = _controller.GetNetworkById(networkId) as NotFoundResult;
//
//             // Assert
//             Assert.That(result, Is.Not.Null);
//             Assert.That(result!.StatusCode, Is.EqualTo(404));
//         }
//
//         [Test]
//         public void CreateNetwork_ShouldReturnCreatedResponse_WhenSuccessful()
//         {
//             // Arrange
//             var createDto = new CreateNetworkDto { Name = "NewNetwork", Description = "", ImageUrl = "", IsPublic = true};
//             var network = new Network { Id = Guid.NewGuid(), Name = "NewNetwork", Description = "", ImageUrl = "", IsPublic = true };
//             var networkDto = new NetworkDto { Id = network.Id, Name = "NewNetwork", Description = network.Description, ImageUrl = network.ImageUrl, IsPublic = true, IsSystemProtected = false };
//
//             _networkServiceMock.Setup(s => s.Create(createDto)).Returns(network);
//             _mapperMock.Setup(m => m.Map<NetworkDto>(network)).Returns(networkDto);
//
//             // Act
//             var result = _controller.CreateNetwork(createDto) as ObjectResult;
//
//             // Assert
//             Assert.That(result, Is.Not.Null);
//             Assert.That(result!.StatusCode, Is.EqualTo(201));
//             Assert.That(result.Value, Is.EqualTo(networkDto));
//         }
//
//         [Test]
//         public void Update_ShouldReturnUpdatedNetwork_WhenSuccessful()
//         {
//             // Arrange
//             var networkId = _networks[0].Id;
//             var updateDto = new UpdateNetworkDto { Name = "UpdatedNetwork", IsPublic = true, Description = "a" };
//             var network = new Network { Id = networkId, Name = "UpdatedNetwork", IsPublic = true, Description = "a", ImageUrl = "a"};
//             var networkDto = new NetworkDto { Id = networkId, Name = "UpdatedNetwork", Description = "", IsPublic = true, IsSystemProtected = false };
//
//             _networkServiceMock.Setup(s => s.Update(networkId, updateDto)).Returns(network);
//             _mapperMock.Setup(m => m.Map<NetworkDto>(network)).Returns(networkDto);
//
//             // Act
//             var result = _controller.Update(networkId, updateDto) as ObjectResult;
//
//             // Assert
//             Assert.That(result, Is.Not.Null);
//             Assert.That(result!.StatusCode, Is.EqualTo(200));
//             Assert.That(result.Value, Is.EqualTo(networkDto));
//         }
//
//         [Test]
//         public void Delete_ShouldReturnNoContent_WhenSuccessful()
//         {
//             // Arrange
//             var networkId = Guid.NewGuid();
//             _networkServiceMock.Setup(s => s.DeleteById(networkId)).Returns(true);
//
//             // Act
//             var result = _controller.Delete(networkId) as NoContentResult;
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<NoContentResult>());
//         }
//
//         [Test]
//         public void Delete_ShouldReturnNotFound_WhenNetworkDoesNotExist()
//         {
//             // Arrange
//             var networkId = Guid.NewGuid();
//             _networkServiceMock.Setup(s => s.DeleteById(networkId)).Returns(false);
//
//             // Act
//             var result = _controller.Delete(networkId);
//
//             // Assert
//             Assert.That(result, Is.InstanceOf<NoContentResult>());
//         }
//     }
// }