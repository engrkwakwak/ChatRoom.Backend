using ChatRoom.Backend.Presentation.Controllers;
using Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.DataTransferObjects.File;

namespace ChatRoom.UnitTest.ControllerTests {
    public class FilesControllerTests {
        private readonly Mock<IFileManager> _fileManagerMock;
        private readonly FilesController _controller;

        public FilesControllerTests() {
            _fileManagerMock = new Mock<IFileManager>();
            _controller = new FilesController(_fileManagerMock.Object) {
                ControllerContext = new ControllerContext {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task UploadImage_WhenImageFileIsNull_ShouldReturnBadRequest() {
            // Arrange
            PictureForUploadDto picture = new() { };

            // Act
            var result = await _controller.UploadImage(picture) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("No image provided.", result.Value);
        }

        [Fact]
        public async Task UploadImage_WhenImageFileLengthIsZero_ShouldReturnBadRequest() {
            // Arrange
            var formFile = new FormFile(Stream.Null, 0, 0, "ImageFile", "image.jpg");

            PictureForUploadDto picture = new() {
                ImageFile = formFile
            };

            // Act
            var result = await _controller.UploadImage(picture) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("No image provided.", result.Value);

            // Verify
            _fileManagerMock.Verify(x => x.UploadImageAsync(picture.ImageFile.OpenReadStream(), picture), Times.Never);
        }

        [Fact]
        public async Task UploadImage_WhenContentTypeDoesNotStartWithImage_ShouldReturnBadRequest() {
            // Arrange
            var fileContent = new MemoryStream(new byte[1]);
            var formFile = new FormFile(fileContent, 0, fileContent.Length, "ImageFile", "text/plain");

            PictureForUploadDto picture = new() {
                ImageFile = formFile,
                ContentType = "text/plain"
            };

            // Act
            var result = await _controller.UploadImage(picture) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid file type. Only image files are allowed.", result.Value);
        }

        [Fact]
        public async Task UploadImage_ShouldReturnTheBlobUri() {
            // Arrange
            var fileContent = new MemoryStream(new byte[1]);
            var formFile = new FormFile(fileContent, 0, fileContent.Length, "ImageFile", "text/plain");
            string uri = "SampleUri";

            PictureForUploadDto picture = new() {
                ImageFile = formFile,
                ContentType = "image/png"
            };

            _fileManagerMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<PictureForUploadDto>()))
                .ReturnsAsync(uri);

            // Act
            var result = await _controller.UploadImage(picture) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal("SampleUri", result.Value);
        }

        [Fact]
        public async Task DeleteImage_ShouldDeleteImageAndReturnNoContent() {
            // Arrange
            PictureForDeletionDto picture = new() { PictureUrl = "sampleurl" };

            _fileManagerMock.Setup(x => x.DeleteImageAsync(It.IsAny<string>()));

            // Act
            var result = await _controller.DeleteImage(picture) as NoContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, result.StatusCode);
        }
    }
}
