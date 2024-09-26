using Pager.Duty.Requests;

namespace Tests;

public class EqualityTests {

    private static void TestEquality<T>(T subj, T same, T diff) where T: notnull {
        subj.Should().Be(same);
        subj.Should().NotBe(diff);

        subj.GetHashCode().Should().Be(same.GetHashCode());
        subj.GetHashCode().Should().NotBe(diff.GetHashCode());
    }

    [Fact]
    public void LinkEquality() => TestEquality(
        subj: new Link("https://aldaviva.freshping.io/reports?check_id=36897", "Freshping Report"),
        same: new Link("https://aldaviva.freshping.io/reports?check_id=36897", "Freshping Report"),
        diff: new Link("https://aldaviva.freshping.io/reports?check_id=36898", "Freshping Report"));

    [Fact]
    public void ImageEquality() => TestEquality(
        subj: new Image("https://aldaviva.com/portfolio/images/title_big.png", "https://aldaviva.com/", "Ben Hutchison"),
        same: new Image("https://aldaviva.com/portfolio/images/title_big.png", "https://aldaviva.com/", "Ben Hutchison"),
        diff: new Image("https://aldaviva.com/portfolio/images/title_big.png", "https://aldaviva.com", "Ben Hutchison"));

}