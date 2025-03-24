using Pager.Duty.Requests;
using System;

namespace Tests;

public class DataTests {

    [Fact]
    public void LinkUris() {
        Link actual = new(new Uri("https://aldaviva.com"), "Aldaviva");
        actual.Href.Should().Be("https://aldaviva.com/");
        actual.Text.Should().Be("Aldaviva");
    }

    [Fact]
    public void ImageUris() {
        Image actual = new(new Uri("https://aldaviva.com/avatars/ben.jpg"), new Uri("https://aldaviva.com"), "Aldaviva");
        actual.Href.Should().Be("https://aldaviva.com/");
        actual.Source.Should().Be("https://aldaviva.com/avatars/ben.jpg");
        actual.AltText.Should().Be("Aldaviva");
    }

}