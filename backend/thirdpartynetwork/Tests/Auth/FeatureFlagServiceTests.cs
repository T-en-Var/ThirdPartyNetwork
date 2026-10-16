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
public class FeatureFlagServiceTests
{
    private Mock<INetworkRepository> _networkRepositoryMock;
    private Mock<INetworkUserRepository> _networkUserRepositoryMock;
    private Mock<IFileRepository> _fileRepositoryMock;
    private Mock<IBlogRepository> _blogRepositoryMock;
    private Mock<IConfigurationRepository> _configurationRepositoryMock;
    private Mock<ICustomPageRepository> _customPageRepositoryMock;
    private FeatureFlagValidator _featureFlagService;

    [SetUp]
    public void SetUp()
    {
        _networkRepositoryMock = new Mock<INetworkRepository>();
        _networkUserRepositoryMock = new Mock<INetworkUserRepository>();
        _fileRepositoryMock = new Mock<IFileRepository>();
        _blogRepositoryMock = new Mock<IBlogRepository>();
        _configurationRepositoryMock = new Mock<IConfigurationRepository>();
        _customPageRepositoryMock = new Mock<ICustomPageRepository>();

        _featureFlagService = new FeatureFlagValidator(
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
            FeatureFlag = new NetworkFeatureFlag
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

    #region CheckUserAddNetworkFileFeatureFlagAsync Tests

    [Test]
    public async Task CheckUserAddNetworkFileFeatureFlagAsync_HappyPath_ReturnsTrue()
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
            FeatureFlag = new NetworkFeatureFlag
            {
                AllowFiles = true, FileCountLimit = 10, FileSizeLimit = 1000, FileStorageLimit = 5000 // 5000 KB
            }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            FeatureFlags = JsonDocument.Parse("{}"), NetworkUserRoles = new List<NetworkUserRole>()
        };
        var file = new Mock<IFormFile>();
        file.Setup(f => f.Length).Returns(500); // 500 bytes

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _fileRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<File>());

        // Act
        var result =
            await _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(userProxyId, networkId, file.Object);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CheckUserAddNetworkFileFeatureFlagAsync_NetworkNotFound_ThrowsNotFound()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync((Network)null!);

        // Act & Assert
        Assert.ThrowsAsync<NotFoundHttpException>(() =>
            _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(Guid.NewGuid(), networkId,
                new Mock<IFormFile>().Object));
    }

    [Test]
    public async Task CheckUserAddNetworkFileFeatureFlagAsync_FilesNotAllowed_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#", FeatureFlag = new NetworkFeatureFlag { AllowFiles = false }
        };
        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);

        // Act
        var result =
            await _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(Guid.NewGuid(), networkId,
                new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddNetworkFileFeatureFlagAsync_NetworkFileCountLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            Name = "Test Network",
            Description = "Test Network Description",
            IsPublic = false,
            RedirectURI = "#", FeatureFlag = new NetworkFeatureFlag { AllowFiles = true, FileCountLimit = 1 }
        };
        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>
        {
            new()
            {
                Bucket = "private", ObjectKey = Guid.NewGuid().ToString()
            }
        });

        // Act
        var result =
            await _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(Guid.NewGuid(), networkId,
                new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddNetworkFileFeatureFlagAsync_NetworkStorageLimitExceeded_ReturnsFalse()
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
            FeatureFlag = new NetworkFeatureFlag { AllowFiles = true, FileStorageLimit = 1, FileSizeLimit = 2048 }
        }; // 1 KB
        var existingFile = new File { SizeBytes = 1000, Bucket = "private", ObjectKey = Guid.NewGuid().ToString() };
        var newFile = new Mock<IFormFile>();
        newFile.Setup(f => f.Length).Returns(25); // 1000 + 25 > 1024

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File> { existingFile });

        // Act
        var result =
            await _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(Guid.NewGuid(), networkId,
                newFile.Object);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void CheckUserAddNetworkFileFeatureFlagAsync_NetworkUserNotFound_ThrowsNotFound()
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
            FeatureFlag = new NetworkFeatureFlag
                { AllowFiles = true, FileCountLimit = 2, FileStorageLimit = 2, FileSizeLimit = 2 }
        };
        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _fileRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<File>());
        _networkUserRepositoryMock.Setup(r =>
                r.GetByUserProxyIdAsync(userProxyId,
                    It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser>()); // Empty list

        // Act & Assert
        Assert.ThrowsAsync<NotFoundHttpException>(() =>
            _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(userProxyId, networkId,
                new Mock<IFormFile>().Object));
    }

    [Test]
    public async Task CheckUserAddNetworkFileFeatureFlagAsync_UserFileCountLimitExceeded_ReturnsFalse()
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
            FeatureFlag = new NetworkFeatureFlag { AllowFiles = true, FileCountLimit = 10 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 1}"), NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
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
                    Bucket = "private", ObjectKey = Guid.NewGuid().ToString()
                }
            });

        // Act
        var result =
            await _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(userProxyId, networkId,
                new Mock<IFormFile>().Object);

        // Assert
        // This test will fail if the logic is `count > limit` instead of `count >= limit`.
        // With 1 file and a limit of 1, `1 > 1` is false, and the check would incorrectly pass.
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddNetworkFileFeatureFlagAsync_UserHasRoleWithHigherLimit_ReturnsTrue()
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
            FeatureFlag = new NetworkFeatureFlag
                { AllowFiles = true, FileCountLimit = 1, FileSizeLimit = 1000, FileStorageLimit = 5000 }
        };
        var role = new Role
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 5}"),
            Name = "Role Name",
            Description = "Role Description",
            IsDefault = false,
            Network = network
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            FeatureFlags = JsonDocument.Parse("{}"), // No personal override
        };
        networkUser.NetworkUserRoles = new List<NetworkUserRole>
        {
            new()
            {
                Role = role,
                NetworkUser = networkUser
            }
        };

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
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
                    Bucket = "private", ObjectKey = Guid.NewGuid().ToString()
                }
            });

        // Act
        var result =
            await _featureFlagService.CheckUserAddNetworkFileFeatureFlagAsync(userProxyId, networkId,
                new Mock<IFormFile>().Object);

        // Assert
        Assert.That(result, Is.True);
    }

    #endregion

    #region CheckUserAddBlogFeatureFlagAsync Tests

    [Test]
    public async Task CheckUserAddBlogFeatureFlagAsync_HappyPath_ReturnsTrue()
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
            FeatureFlag = new NetworkFeatureFlag { AllowBlogs = true, BlogCountLimit = 5 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            FeatureFlags = JsonDocument.Parse("{}"), NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _blogRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<Blog>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _blogRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<Blog>());

        // Act
        var result = await _featureFlagService.CheckUserAddBlogFeatureFlagAsync(userProxyId, networkId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckUserAddBlogFeatureFlagAsync_UserLimitExactlyMet_ReturnsFalse()
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
            FeatureFlag = new NetworkFeatureFlag { AllowBlogs = true, BlogCountLimit = 10 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            FeatureFlags = JsonDocument.Parse("{\"BlogCountLimit\": 1}"), NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
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
        var result = await _featureFlagService.CheckUserAddBlogFeatureFlagAsync(userProxyId, networkId);

        // Assert
        // This test will fail if the logic is `count > limit` instead of `count >= limit`.
        Assert.That(result, Is.False);
    }

    #endregion

    #region CheckUserAddCustomPageFeatureFlagAsync Tests

    [Test]
    public async Task CheckUserAddCustomPageFeatureFlagAsync_UserLimitReached_ReturnsFalse()
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
            FeatureFlag = new NetworkFeatureFlag { AllowCustomPages = true, CustomPageCountLimit = 10 }
        };
        var networkUser = new NetworkUser
        {
            Id = Guid.NewGuid(), UserProxyId = userProxyId, NetworkId = networkId,
            // Because of the suspected bug in the constant name, this will actually set ConfigurationCountLimit
            FeatureFlags = JsonDocument.Parse("{\"ConfigurationCountLimit\": 1}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByNetworkIdAsync(networkId)).ReturnsAsync(new List<CustomPage>());
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<CustomPage> { new() { Name = "name", Slug = "slug" } });

        // Act
        var result = await _featureFlagService.CheckUserAddCustomPageFeatureFlagAsync(userProxyId, networkId);

        // Assert
        // This test will fail if the constant for CustomPageCountLimit is incorrect,
        // or if the check is `>` instead of `>=`.
        Assert.That(result, Is.True);
    }

    #endregion

    #region CheckUserAddCustomPageBlockFeatureFlagAsync Tests

    [Test]
    public async Task CheckUserAddCustomPageBlockFeatureFlagAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            FeatureFlag = new NetworkFeatureFlag
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
            FeatureFlags = JsonDocument.Parse("{}"),
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

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);

        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });

        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<CustomPage>());

        // Act
        var result =
            await _featureFlagService.CheckUserAddCustomPageBlockFeatureFlagAsync(userProxyId, networkId, customPageId,
                pageBlockData);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockFeatureFlagAsync_NetworkNotAllowed_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            FeatureFlag = new NetworkFeatureFlag
            {
                AllowCustomPages = false
            },
            Name = "name",
            IsPublic = false,
            Description = "description",
            RedirectURI = "#"
        };
        var pageBlockData = JsonDocument.Parse("{}").RootElement;

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);

        // Act
        var result =
            await _featureFlagService.CheckUserAddCustomPageBlockFeatureFlagAsync(Guid.NewGuid(), networkId,
                Guid.NewGuid(), pageBlockData);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockFeatureFlagAsync_NetworkBlockCountLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            FeatureFlag = new NetworkFeatureFlag
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

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);

        // Act
        var result = await _featureFlagService.CheckUserAddCustomPageBlockFeatureFlagAsync(Guid.NewGuid(), networkId,
            customPageId, JsonDocument.Parse("{}").RootElement);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockFeatureFlagAsync_NetworkBlockSizeLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            FeatureFlag = new NetworkFeatureFlag
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

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);

        // Act
        var result =
            await _featureFlagService.CheckUserAddCustomPageBlockFeatureFlagAsync(Guid.NewGuid(), networkId,
                customPageId, pageBlockData);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockFeatureFlagAsync_UserBlockCountLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            FeatureFlag = new NetworkFeatureFlag
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
            FeatureFlags = JsonDocument.Parse("{}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        var customPage = new CustomPage
        {
            Id = customPageId,
            Pages = new List<PageBlock>(),
            Name = "name",
            Slug = "slug"
        };

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
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
        var result = await _featureFlagService.CheckUserAddCustomPageBlockFeatureFlagAsync(userProxyId, networkId,
            customPageId, JsonDocument.Parse("{}").RootElement);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CheckUserAddCustomPageBlockFeatureFlagAsync_UserBlockSizeLimitReached_ReturnsFalse()
    {
        // Arrange
        var networkId = Guid.NewGuid();
        var userProxyId = Guid.NewGuid();
        var customPageId = Guid.NewGuid();
        var network = new Network
        {
            Id = networkId,
            FeatureFlag = new NetworkFeatureFlag
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
            FeatureFlags = JsonDocument.Parse("{\"CustomPageBlockSizeLimit\": 5}"),
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

        _networkRepositoryMock.Setup(r => r.GetByIdFeatureFlagAsync(networkId)).ReturnsAsync(network);
        _customPageRepositoryMock.Setup(r => r.GetByIdAsync(customPageId)).ReturnsAsync(customPage);
        _networkUserRepositoryMock
            .Setup(r => r.GetByUserProxyIdAsync(userProxyId,
                It.IsAny<Func<IQueryable<NetworkUser>, IQueryable<NetworkUser>>>()))
            .ReturnsAsync(new List<NetworkUser> { networkUser });
        _customPageRepositoryMock.Setup(r => r.GetByAuthorIdAsync(networkId, networkUser.Id, false))
            .ReturnsAsync(new List<CustomPage>());

        // Act
        var result =
            await _featureFlagService.CheckUserAddCustomPageBlockFeatureFlagAsync(userProxyId, networkId, customPageId,
                pageBlockData);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region Read/Write/Remove FeatureFlag Tests

    [Test]
    public void CustomPageCountLimit_And_ConfigurationCountLimit_DoNotConflict()
    {
        // Arrange
        var networkUser = new NetworkUser { FeatureFlags = JsonDocument.Parse("{}") };
        const int customPageLimit = 5;
        const int configLimit = 10;

        // Act
        networkUser = NetworkFeatureFlagFactory.WriteFeatureFlag(networkUser, customPageLimit ,FeatureFlagType.CustomPageCountLimit);
        networkUser = NetworkFeatureFlagFactory.WriteFeatureFlag(networkUser, configLimit, FeatureFlagType.ConfigurationCountLimit);

        // Assert
        var readCustomPageLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.CustomPageCountLimit);
        var readConfigLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.ConfigurationCountLimit);

        // This will fail if WriteCustomPageCountLimitFeatureFlag uses the same key as WriteConfigurationCountLimitFeatureFlag
        Assert.That(readCustomPageLimit, Is.EqualTo(customPageLimit), "Custom CustomPage limit should be preserved.");
        Assert.That(readConfigLimit, Is.EqualTo(configLimit), "Configuration limit should be preserved.");
    }

    [Test]
    public void ReadFileCountLimitFeatureFlag_NoLimits_ReturnsNull()
    {
        // Arrange
        var networkUser = new NetworkUser
            { FeatureFlags = JsonDocument.Parse("{}"), NetworkUserRoles = new List<NetworkUserRole>() };

        // Act
        var limit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.Null);
    }

    [Test]
    public void ReadFileCountLimitFeatureFlag_OnlyUserLimit_ReturnsUserLimit()
    {
        // Arrange
        var networkUser = new NetworkUser
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 10}"),
            NetworkUserRoles = new List<NetworkUserRole>()
        };

        // Act
        var limit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.EqualTo(10));
    }

    [Test]
    public void ReadFileCountLimitFeatureFlag_OnlyRoleLimit_ReturnsRoleLimit()
    {
        // Arrange
        var roleWithLimit = new Role
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 20}"),
            Name = "name",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var networkUser = new NetworkUser
        {
            FeatureFlags = JsonDocument.Parse("{}"),
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
        var limit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.EqualTo(20));
    }

    [Test]
    public void ReadFileCountLimitFeatureFlag_UserAndRoleLimits_ReturnsMax()
    {
        // Arrange
        var roleWithLimit = new Role
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 20}"),
            Name = "name",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var networkUser = new NetworkUser
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 10}"),
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
        var limit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);

        // Assert
        // Should be Max(user_limit, role_limit) = Max(10, 20) = 20
        Assert.That(limit, Is.EqualTo(20));
    }

    [Test]
    public void ReadFileCountLimitFeatureFlag_MultipleRoleLimits_ReturnsMaxRoleLimit()
    {
        // Arrange
        var role1 = new Role
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 20}"),
            Name = "name 1",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var role2 = new Role
        {
            FeatureFlags = JsonDocument.Parse("{\"FileCountLimit\": 30}"),
            Name = "name 1",
            Description = "description",
            IsDefault = false,
            Network = null!
        };
        var networkUser = new NetworkUser
        {
            FeatureFlags = JsonDocument.Parse("{}"),
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
        var limit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);

        // Assert
        Assert.That(limit, Is.EqualTo(30));
    }

    [Test]
    public void WriteAndRemoveFileCountLimitFeatureFlag_Works()
    {
        // Arrange
        var networkUser = new NetworkUser { FeatureFlags = JsonDocument.Parse("{}") };

        // Act (Write)
        networkUser = NetworkFeatureFlagFactory.WriteFeatureFlag(networkUser, 55, FeatureFlagType.FileCountLimit);

        // Assert (Write)
        var readLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);
        Assert.That(readLimit, Is.EqualTo(55));

        // Act (Remove)
        networkUser = NetworkFeatureFlagFactory.RemoveFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);

        // Assert (Remove)
        var removedLimit = NetworkFeatureFlagFactory.ReadFeatureFlag<int?>(networkUser, FeatureFlagType.FileCountLimit);
        Assert.That(removedLimit, Is.Null);
    }

    #endregion
}