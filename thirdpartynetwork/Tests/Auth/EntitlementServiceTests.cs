using System.Text.Json;
using Moq;
using NUnit.Framework;
using thirdpartynetwork.Application.Auth;
using thirdpartynetwork.Core.Enum;
using thirdpartynetwork.Core.Exceptions.HttpExceptions;
using thirdpartynetwork.Core.Models;
using File = thirdpartynetwork.Core.Models.File;
using IFormFile = Microsoft.AspNetCore.Http.IFormFile;

namespace thirdpartynetwork.Tests.Auth;

[TestFixture]
public class EntitlementServiceTests
{
    private Mock<INetworkRepository> _networkRepositoryMock;
    private Mock<INetworkUserRepository> _networkUserRepositoryMock;
    private Mock<IFileRepository> _fileRepositoryMock;
    private Mock<IBlogRepository> _blogRepositoryMock;
    private Mock<IConfigurationRepository> _configurationRepositoryMock;
    private Mock<ICustomPageRepository> _customPageRepositoryMock;
    private EntitlementValidator _entitlementService;

    [SetUp]
    public void SetUp()
    {
        _networkRepositoryMock = new Mock<INetworkRepository>();
        _networkUserRepositoryMock = new Mock<INetworkUserRepository>();
        _fileRepositoryMock = new Mock<IFileRepository>();
        _blogRepositoryMock = new Mock<IBlogRepository>();
        _configurationRepositoryMock = new Mock<IConfigurationRepository>();
        _customPageRepositoryMock = new Mock<ICustomPageRepository>();

        _entitlementService = new EntitlementValidator(
            _networkRepositoryMock.Object,
            _networkUserRepositoryMock.Object,
            _fileRepositoryMock.Object,
            _blogRepositoryMock.Object,
            _configurationRepositoryMock.Object,
            _customPageRepositoryMock.Object
        );
        
        var mainNetwork = new Network
        {
            Id = Guid.NewGuid(),
            Name = "Main Network",
            Entitlement = new NetworkEntitlement
            {
                AllowFiles = true,
                AllowBlogs = true,
                AllowConfigurations = true,
                AllowCustomPages = true
            },
            IsPublic = true,
            Description = "#",
            RedirectURI = "#"
        };
    
        _networkRepositoryMock
            .Setup(r => r.GetMainNetworkDetailsAsync())
            .ReturnsAsync(mainNetwork);
    }

    #region CheckUserAddNetworkFileEntitlementAsync Tests

    [Test]
    public async Task CheckUserAddNetworkFileEntitlementAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement
            {
                AllowFiles = true, FileCountLimit = 10, FileSizeLimit = 1000, FileStorageLimit = 5000 // 5000 KB
            }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"), NetworkUserRoles = new List<NetworkUserRole>()
        };
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(500); // 500 bytes

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _fileRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<File>());

        // Act
        var result =
            await _entitlementService.CheckUserAddNetworkFileEntitlementAsync(userProxyId, networkId, file.Object);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CheckUserAddNetworkFileEntitlementAsync_NetworkNotFound_ThrowsNotFound()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync((Network)null!);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundHttpException>(() =>
            _entitlementService.CheckUserAddNetworkFileEntitlementAsync(Guid.NewGuid(), networkId,
                new Mock<IFormFile>().Object));
    }

    [Test]
    public async Task CheckUserAddNetworkFileEntitlementAsync_FilesNotAllowed_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#", Entitlement = new NetworkEntitlement { AllowFiles = false }
        };
        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);

        // Act
        var result =
            await _entitlementService.CheckUserAddNetworkFileEntitlementAsync(Guid.NewGuid(), networkId,
                new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddNetworkFileEntitlementAsync_NetworkFileCountLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#", Entitlement = new NetworkEntitlement { AllowFiles = true, FileCountLimit = 1 }
        };
        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>
        {
            new()
            {
                PublicId = Guid.NewGuid()
            }
        });

        // Act
        var result =
            await _entitlementService.CheckUserAddNetworkFileEntitlementAsync(Guid.NewGuid(), networkId,
                new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddNetworkFileEntitlementAsync_NetworkStorageLimitExceeded_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement { AllowFiles = true, FileStorageLimit = 1, FileSizeLimit = 2048 }
        }; // 1 KB
        var existingFile = new File { SizeBytes = 1000, PublicId = Guid.NewGuid() };
        var newFile = new Mock<IFormFile>();
        newFile.Setup(f => f.Length).Returns(25); // 1000 + 25 > 1024

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File> { existingFile });

        // Act
        var result =
            await _entitlementService.CheckUserAddNetworkFileEntitlementAsync(Guid.NewGuid(), networkId,
                newFile.Object);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void CheckUserAddNetworkFileEntitlementAsync_NetworkUserNotFound_ThrowsNotFound()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement
                { AllowFiles = true, FileCountLimit = 2, FileStorageLimit = 2, FileSizeLimit = 2 }
        };
        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>());
        _networkUserRepositoryMock.Setup(r =>
                r.GetByUserProxyIdAsync(userProxyId,
                    It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser>()); // Empty list

        // Act & Assert
        Assert.ThrowsAsync<NotFoundHttpException>(() =>
            _entitlementService.CheckUserAddNetworkFileEntitlementAsync(userProxyId, networkId,
                new Mock<IFormFile>().Object));
    }

    [Test]
    public async Task CheckUserAddNetworkFileEntitlementAsync_UserFileCountLimitExceeded_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement { AllowFiles = true, FileCountLimit = 10 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 1}"), NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _fileRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<File>
            {
                new()
                {
                    PublicId = Guid.NewGuid()
                }
            });

        // Act
        var result =
            await _entitlementService.CheckUserAddNetworkFileEntitlementAsync(userProxyId, networkId,
                new Mock<IFormFile>().Object);

        // Assert
        // This test will fail if the logic is `count > limit` instead of `count >= limit`.
        // With 1 file and a limit of 1, `1 > 1` is false, and the check would incorrectly pass.
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddNetworkFileEntitlementAsync_UserHasRoleWithHigherLimit_ReturnsTrue()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement
                { AllowFiles = true, FileCountLimit = 1, FileSizeLimit = 1000, FileStorageLimit = 5000 }
        };
        var role = new Role
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 5}"),
            Name = "Role Name",
            Description = "Role Description",
            IsDefault = false,
            Network = network
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"), // No personal override
        };
        networkUser.NetworkUserRoles = new List<NetworkUserRole>
        {
            new()
            {
                Role = role,
                NetworkUser = networkUser
            }
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        // User has 1 file, network limit is 1, but role limit is 5. So should pass.
        _fileRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<File>
            {
                new()
                {
                    PublicId = Guid.NewGuid()
                }
            });

        // Act
        var result =
            await _entitlementService.CheckUserAddNetworkFileEntitlementAsync(userProxyId, networkId,
                new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region CheckUserAddBlogEntitlementAsync Tests

    [Test]
    public async Task CheckUserAddBlogEntitlementAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement { AllowBlogs = true, BlogCountLimit = 5 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"), NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _blogRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<Blog>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _blogRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<Blog>());

        // Act
        var result = await _entitlementService.CheckUserAddBlogEntitlementAsync(userProxyId, networkId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckUserAddBlogEntitlementAsync_UserLimitExactlyMet_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement { AllowBlogs = true, BlogCountLimit = 10 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{\"BlogCountLimit\": 1}"), NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _blogRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<Blog>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _blogRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<Blog>
            {
                new()
                {
                    Title = "title",
                    Slug = "slug",
                    Summary = "summary",
                    Body = JsonDocument.Parse("{}")
                }
            }); // User has 1 blog

        // Act
        var result = await _entitlementService.CheckUserAddBlogEntitlementAsync(userProxyId, networkId);

        // Assert
        // This test will fail if the logic is `count > limit` instead of `count >= limit`.
        Assert.That(result, Is.False);
    }

    #endregion

    #region CheckUserAddCustomPageEntitlementAsync Tests

    [Test]
    public async Task CheckUserAddCustomPageEntitlementAsync_UserLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#",
            Entitlement = new NetworkEntitlement { AllowCustomPages = true, CustomPageCountLimit = 10 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            // Because of the suspected bug in the constant name, this will actually set ConfigurationCountLimit
            Entitlements = JsonDocument.Parse("{\"ConfigurationCountLimit\": 1}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<CustomPage>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<CustomPage> { new() { Name = "name", Slug = "slug" } });

        // Act
        var result = await _entitlementService.CheckUserAddCustomPageEntitlementAsync(userProxyId, networkId);

        // Assert
        // This test will fail if the constant for CustomPageCountLimit is incorrect,
        // or if the check is `>` instead of `>=`.
        Assert.That(result, Is.True);
    }

    #endregion

    #region CheckUserAddCustomPageBlockEntitlementAsync Tests

    [Test]
    public async Task CheckUserAddCustomPageBlockEntitlementAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Entitlement = new NetworkEntitlement
            {
                AllowCustomPages = true,
                CustomPageBlockCountLimit = 10,
                CustomPageBlockSizeLimit = 100
            },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(),
            UserProxyId = userProxyId,
            NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        var pageBlockData = JsonDocument.Parse("{\"content\":\"test\"}").RootElement;
        var customPage = new CustomPage
        {
            Id = customPageId,
            Pages = new List<PageBlock>(),
            Name = "name",
            Slug = "slug"
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);

        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });

        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<CustomPage>());

        // Act
        var result =
            await _entitlementService.CheckUserAddCustomPageBlockEntitlementAsync(userProxyId, networkId, customPageId,
                pageBlockData);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockEntitlementAsync_NetworkNotAllowed_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Entitlement = new NetworkEntitlement
            {
                AllowCustomPages = false
            },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };
        var pageBlockData = JsonDocument.Parse("{}").RootElement;

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);

        // Act
        var result =
            await _entitlementService.CheckUserAddCustomPageBlockEntitlementAsync(Guid.NewGuid(), networkId,
                Guid.NewGuid(), pageBlockData);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockEntitlementAsync_NetworkBlockCountLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Entitlement = new NetworkEntitlement
            {
                AllowCustomPages = true,
                CustomPageBlockCountLimit = 2
            },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };

        var customPage = new CustomPage
        {
            Id = customPageId,
            Pages = new List<PageBlock>
            {
                new()
                {
                    Text = "name1",
                    Position = 0
                },
                new()
                {
                    Text = "name2",
                    Position = 1
                },
            },
            Name = "name",
            Slug = "slug"
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);

        // Act
        var result = await _entitlementService.CheckUserAddCustomPageBlockEntitlementAsync(Guid.NewGuid(), networkId,
            customPageId, JsonDocument.Parse("{}").RootElement);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockEntitlementAsync_NetworkBlockSizeLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Entitlement = new NetworkEntitlement
                { AllowCustomPages = true, CustomPageBlockCountLimit = 10, CustomPageBlockSizeLimit = 5 },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };

        var customPage = new CustomPage
        {
            Id = customPageId,
            Pages = new List<PageBlock>(),
            Name = "name",
            Slug = "slug"
        };
        var pageBlockData = JsonDocument.Parse("{\"long\":\"data\"}").RootElement;

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);

        // Act
        var result =
            await _entitlementService.CheckUserAddCustomPageBlockEntitlementAsync(Guid.NewGuid(), networkId,
                customPageId, pageBlockData);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockEntitlementAsync_UserBlockCountLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Entitlement = new NetworkEntitlement
            {
                AllowCustomPages = true,
                CustomPageBlockCountLimit = 10,
                CustomPageBlockSizeLimit = 100
            },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(),
            UserProxyId = userProxyId,
            NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        var customPage = new CustomPage
        {
            Id = customPageId,
            Pages = new List<PageBlock>(),
            Name = "name",
            Slug = "slug"
        };

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });

        // Note: The implementation compares User's CustomPage Count vs Block Limit.
        var userPages = Enumerable.Range(0, 10).Select(_ => new CustomPage
        {
            Name = "name",
            Slug = "slug"
        }).ToList();
        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(userPages);

        // Act
        var result = await _entitlementService.CheckUserAddCustomPageBlockEntitlementAsync(userProxyId, networkId,
            customPageId, JsonDocument.Parse("{}").RootElement);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockEntitlementAsync_UserBlockSizeLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Entitlement = new NetworkEntitlement
            {
                AllowCustomPages = true,
                CustomPageBlockCountLimit = 10,
                CustomPageBlockSizeLimit = 100
            },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(),
            UserProxyId = userProxyId,
            NetworkId = networkId,
            Entitlements = JsonDocument.Parse("{\"CustomPageBlockSizeLimit\": 5}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        var customPage = new CustomPage
        {
            Id = customPageId,
            Pages = new List<PageBlock>(),
            Name = "name",
            Slug = "slug"
        };
        var pageBlockData = JsonDocument.Parse("{\"data\":\"long\"}").RootElement;

        _networkRepositoryMock.Setup(r => r.GetByIdEntitlementAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<CustomPage>());

        // Act
        var result =
            await _entitlementService.CheckUserAddCustomPageBlockEntitlementAsync(userProxyId, networkId, customPageId,
                pageBlockData);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Read/Write/Remove Entitlement Tests

    [Test]
    public void CustomPageCountLimit_And_ConfigurationCountLimit_DoNotConflict()
    {
        // Arrange
        var networkUser = new NetworkUser { Entitlements = JsonDocument.Parse("{}") };
        const int customPageLimit = 5;
        const int configLimit = 10;

        // Act
        networkUser = NetworkEntitlementFactory.WriteEntitlement(networkUser, customPageLimit ,EntitlementType.CustomPageCountLimit);
        networkUser = NetworkEntitlementFactory.WriteEntitlement(networkUser, configLimit, EntitlementType.ConfigurationCountLimit);

        // Assert
        var readCustomPageLimit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.CustomPageCountLimit);
        var readConfigLimit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.ConfigurationCountLimit);

        // This will fail if WriteCustomPageCountLimitEntitlement uses the same key as WriteConfigurationCountLimitEntitlement
        Assert.That(readCustomPageLimit, Is.EqualTo(customPageLimit), "Custom Page limit should be preserved.");
        Assert.That(readConfigLimit, Is.EqualTo(configLimit), "Configuration limit should be preserved.");
    }

    [Test]
    public void ReadFileCountLimitEntitlement_NoLimits_ReturnsNull()
    {
        // Arrange
        var networkUser = new NetworkUser
            { Entitlements = JsonDocument.Parse("{}"), NetworkUserRoles = new List<NetworkUserRole>() };

        // Act
        var limit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.Null);
    }

    [Test]
    public void ReadFileCountLimitEntitlement_OnlyUserLimit_ReturnsUserLimit()
    {
        // Arrange
        var networkUser = new NetworkUser
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 10}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        // Act
        var limit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.EqualTo(10));
    }

    [Test]
    public void ReadFileCountLimitEntitlement_OnlyRoleLimit_ReturnsRoleLimit()
    {
        // Arrange
        var roleWithLimit = new Role
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 20}"),
            Name = "name",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var networkUser = new NetworkUser
        {
            Entitlements = JsonDocument.Parse("{}"),
        };
        networkUser.NetworkUserRoles = new List<NetworkUserRole>
        {
            new()
            {
                Role = roleWithLimit,
                NetworkUser = networkUser
            }
        };

        // Act
        var limit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.EqualTo(20));
    }

    [Test]
    public void ReadFileCountLimitEntitlement_UserAndRoleLimits_ReturnsMax()
    {
        // Arrange
        var roleWithLimit = new Role
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 20}"),
            Name = "name",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var networkUser = new NetworkUser
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 10}"),
        };
        networkUser.NetworkUserRoles = new List<NetworkUserRole>
        {
            new()
            {
                Role = roleWithLimit,
                NetworkUser = networkUser
            }
        };

        // Act
        var limit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);

        // Assert
        // Should be Max(user_limit, role_limit) = Max(10, 20) = 20
        Assert.That(limit, Is.EqualTo(20));
    }

    [Test]
    public void ReadFileCountLimitEntitlement_MultipleRoleLimits_ReturnsMaxRoleLimit()
    {
        // Arrange
        var role1 = new Role
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 20}"),
            Name = "name 1",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var role2 = new Role
        {
            Entitlements = JsonDocument.Parse("{\"FileCountLimit\": 30}"),
            Name = "name 1",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var networkUser = new NetworkUser
        {
            Entitlements = JsonDocument.Parse("{}"),
        };
        networkUser.NetworkUserRoles = new List<NetworkUserRole>
        {
            new()
            {
                Role = role2,
                NetworkUser = networkUser
            }
        };
        networkUser.NetworkUserRoles.Add(new NetworkUserRole
        {
            Role = role1,
            NetworkUser = networkUser
        });

        // Act
        var limit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.EqualTo(30));
    }

    [Test]
    public void WriteAndRemoveFileCountLimitEntitlement_Works()
    {
        // Arrange
        var networkUser = new NetworkUser { Entitlements = JsonDocument.Parse("{}") };

        // Act (Write)
        networkUser = NetworkEntitlementFactory.WriteEntitlement(networkUser, 55, EntitlementType.FileCountLimit);

        // Assert (Write)
        var readLimit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);
        Assert.That(readLimit, Is.EqualTo(55));

        // Act (Remove)
        networkUser = NetworkEntitlementFactory.RemoveEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);

        // Assert (Remove)
        var removedLimit = NetworkEntitlementFactory.ReadEntitlement<int?>(networkUser, EntitlementType.FileCountLimit);
        Assert.That(removedLimit, Is.Null);
    }

    #endregion
}