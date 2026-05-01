using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalYearProjectCore.Services;
using FinalYearProjectCore.Interfaces;
using FinalYearProjectCore.Helper;
using FinalYearProjectCore.Model;
using FinalYearProjectCore.ViewModels;
using FinalYearProjectCore.Database;
using Firebase.Auth;


namespace CarbonCompassTests
{
    //Naming convention
    //MethodName_WhichStateIsUnderTest_ExpectedResult
    public class MainViewModelTests
    {
        private readonly Mock<ISettingsService> _mockSettingsService;
        private readonly Mock<IProfileService> _mockProfileService;
        private readonly Mock<FirebaseAuthClient> _mockAuthClient;

        public MainViewModelTests()
        {
            //set up mock
            _mockSettingsService = new Mock<ISettingsService>();
            _mockProfileService = new Mock<IProfileService>();
            _mockAuthClient = new Mock<FirebaseAuthClient>();
        }

        //create SystemUnderTest (SUT)
        private MainViewModel CreateSut()
        {
            return new MainViewModel(_mockSettingsService.Object, _mockProfileService.Object, _mockAuthClient.Object);
        }

        //constructor tests
        [Fact]
        public void Constructor_WhenInstantiated_InitializesEmptyItemsCollection()
        {
            var sut = CreateSut();

            Assert.NotNull(sut.Items);
            Assert.Empty(sut.Items);
        }

        //Add command tests
        [Fact]
        public void AddCommand_WhenTextIsValid_AddsItemToCollection()
        {
            var sut = CreateSut();
            sut.Text = "Test Item";

            sut.AddCommand.Execute(null);

            Assert.Single(sut.Items);
            Assert.Contains("Test Item", sut.Items);
        }

        [Fact]
        public void AddCommand_WhenTextIsValid_ClearsTextPropertyAfterAdding()
        {
            var sut = CreateSut();
            sut.Text = "Hello";

            sut.AddCommand.Execute(null);

            Assert.Empty(sut.Text);
        }

        [Fact]
        public void AddCommand_WhenCalledMultipleTimes_AddsAllValidItemsToCollection()
        {
            var sut = CreateSut();

            sut.Text = "Alpha";
            sut.AddCommand.Execute(null);

            sut.Text = "Beta";
            sut.AddCommand.Execute(null);

            Assert.Equal(2, sut.Items.Count);
            Assert.Equal("Alpha", sut.Items[0]);
            Assert.Equal("Beta", sut.Items[1]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddCommand_WhenTextIsNullOrWhiteSpace_DoesNotAddItem(string invalidText)
        {
            var sut = CreateSut();
            sut.Text = invalidText;

            sut.AddCommand.Execute(null);

            Assert.Empty(sut.Items);
            Assert.Equal(invalidText, sut.Text); //ensure the text wasn't accidentally cleared
        }

        //delete command tests

        [Fact]
        public void DeleteCommand_WhenItemExists_RemovesItFromCollection()
        {
            var sut = CreateSut();
            sut.Items.Add("ToDelete");
            sut.Items.Add("ToKeep");

            sut.DeleteCommand.Execute("ToDelete");

            Assert.Single(sut.Items);
            Assert.DoesNotContain("ToDelete", sut.Items);
            Assert.Contains("ToKeep", sut.Items);
        }

        [Theory]
        [InlineData("GhostItem")]
        [InlineData("")]
        [InlineData(null)]
        public void DeleteCommand_WhenItemDoesNotExist_LeavesCollectionUnchanged(string invalidItem)
        {
            var sut = CreateSut();
            sut.Items.Add("Keep");

            sut.DeleteCommand.Execute(invalidItem);

            Assert.Single(sut.Items);
            Assert.Contains("Keep", sut.Items);
        }

        [Fact]
        public void DeleteCommand_WhenCollectionHasDuplicates_RemovesOnlyFirstOccurrence()
        {
            var sut = CreateSut();
            sut.Items.Add("Duplicate");
            sut.Items.Add("Duplicate");

            sut.DeleteCommand.Execute("Duplicate");

            Assert.Single(sut.Items);
            Assert.Contains("Duplicate", sut.Items);
        }
    }
}
