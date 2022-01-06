using NUnit.Framework;
using Mutateful.Core;
using Mutateful.Utility;

namespace MutatefulTests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestDecimalToSpreadshimal()
    {
        Assert.AreEqual("", ClipReference.ToSpreadshimal(0));
        Assert.AreEqual("A", ClipReference.ToSpreadshimal(1));
        Assert.AreEqual("B", ClipReference.ToSpreadshimal(2));
        Assert.AreEqual("Z", ClipReference.ToSpreadshimal(26));
        Assert.AreEqual("AA", ClipReference.ToSpreadshimal(27));
        Assert.AreEqual("AC", ClipReference.ToSpreadshimal(29));
        Assert.AreEqual("ZZ", ClipReference.ToSpreadshimal(702));
        Assert.AreEqual("AAA", ClipReference.ToSpreadshimal(703));
        Assert.AreEqual("AAB", ClipReference.ToSpreadshimal(704));
    }        
    
    [Test]
    public void TestSpreadshimalToDecimal()
    {
        Assert.AreEqual(0, ClipReference.FromSpreadshimal(""));
        Assert.AreEqual(1, ClipReference.FromSpreadshimal("A"));
        Assert.AreEqual(2, ClipReference.FromSpreadshimal("B"));
        Assert.AreEqual(26, ClipReference.FromSpreadshimal("Z"));
        Assert.AreEqual(27, ClipReference.FromSpreadshimal("AA"));
        Assert.AreEqual(29, ClipReference.FromSpreadshimal("AC"));
        Assert.AreEqual(702, ClipReference.FromSpreadshimal("ZZ"));
        Assert.AreEqual(703, ClipReference.FromSpreadshimal("AAA"));
        Assert.AreEqual(704, ClipReference.FromSpreadshimal("AAB"));
        Assert.AreEqual(702, ClipReference.FromSpreadshimal("zz"));
        Assert.AreEqual(703, ClipReference.FromSpreadshimal("aaa"));
        Assert.AreEqual(704, ClipReference.FromSpreadshimal("aab"));
    }

    [Test]
    public void GetClipDataForTest()
    {
        var clip = new Clip(4m, true)
        {
            Notes = new SortedList<NoteEvent>
            {
                new (36, 0, .25m, 100),
                new (48, .5m, .15m, 100),
                new (60, 1, .3m, 100),
                new (65, 2, 2, 90)
            }
        };
        var data = IOUtilities.GetClipAsBytesLive11(clip);
        Console.WriteLine(string.Join(',', data));
        var clip2 = Mutateful.IO.Decoder.GetSingleLive11Clip(data.ToArray());
        Assert.AreEqual(clip.Notes.Count, clip2.Notes.Count);
        Assert.AreEqual(clip.Notes[3].Start, clip2.Notes[3].Start);
    }

    [Test]
    public void TestNormalizeClipLengths()
    {
        var clip1 = new Clip(1, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(60, 0, .25m, 100),
                new NoteEvent(60, .4m, .1m, 100),
            }
        };
        var clip2 = new Clip(4, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(62, 0, 1, 100),
                new NoteEvent(62, 1, 1, 100),
                new NoteEvent(62, 2, 1, 100),
                new NoteEvent(62, 3, 1, 100)
            }
        };
        var clip3 = new Clip(2, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(64, .5m, 1, 100),
                new NoteEvent(64, 1.5m, .5m, 100),
            }
        };
        ClipUtilities.NormalizeClipLengths(new[] { clip1, clip2, clip3 });
        Assert.AreEqual(clip1.Notes[0].Start, 0);
        Assert.AreEqual(clip1.Notes[1].Start, .4m);
        Assert.AreEqual(clip1.Notes[2].Start, 1);
        Assert.AreEqual(clip1.Notes[3].Start, 1.4m);
        Assert.AreEqual(clip1.Notes[4].Start, 2);
        Assert.AreEqual(clip1.Notes[5].Start, 2.4m);
        Assert.AreEqual(clip1.Notes[6].Start, 3);
        Assert.AreEqual(clip1.Notes[7].Start, 3.4m);
        Assert.AreEqual(clip1.Notes.Count, 8);
        Assert.AreEqual(clip2.Notes[0].Start, 0);
        Assert.AreEqual(clip2.Notes[1].Start, 1);
        Assert.AreEqual(clip2.Notes[2].Start, 2);
        Assert.AreEqual(clip2.Notes[3].Start, 3);
        Assert.AreEqual(clip2.Notes.Count, 4);
        Assert.AreEqual(clip3.Notes[0].Start, .5m);
        Assert.AreEqual(clip3.Notes[1].Start, 1.5m);
        Assert.AreEqual(clip3.Notes[2].Start, 2.5m);
        Assert.AreEqual(clip3.Notes[3].Start, 3.5m);
        Assert.AreEqual(clip3.Notes.Count, 4);
    }

    [Test]
    public void TestMonophonize()
    {
        var clip1 = new Clip(1, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(60, 0, 1, 100),
                new NoteEvent(62, 0, 0.2m, 100),
                new NoteEvent(62, 0.3m, 0.2m, 100),
                new NoteEvent(62, 0.5m, 0.2m, 100),
                new NoteEvent(62, 0.8m, 0.2m, 100),
                new NoteEvent(60, 1, 1, 100),
            }
        };
        Assert.AreEqual(6, clip1.Notes.Count);
        ClipUtilities.Monophonize(clip1);
        Assert.AreEqual(2, clip1.Notes.Count);

        clip1 = new Clip(1, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(60, 0, .2m, 100),
                new NoteEvent(62, .1m, .2m, 100),
                new NoteEvent(62, .2m, .2m, 100)
            }
        };
        Assert.AreEqual(3, clip1.Notes.Count);
        ClipUtilities.Monophonize(clip1);
        Assert.AreEqual(2, clip1.Notes.Count);
    }

    [Test]
    public void TestRoundUpToNearestSixteenth()
    {
        var val = 1.22m;
        Assert.AreEqual(1.25m, Utilities.RoundUpToNearestSixteenth(val));
        val = 1.25m;
        Assert.AreEqual(1.25m, Utilities.RoundUpToNearestSixteenth(val));
        val = 0.51m;
        Assert.AreEqual(0.75m, Utilities.RoundUpToNearestSixteenth(val));
    }
        
    [Test]
    public void TestRecordClipsAndNoteEvents()
    {
        var clip = new Clip(4m, true)
        {
            Notes = new SortedList<NoteEvent>
            {
                new (36, 0, .25m, 100),
                new (48, .5m, .15m, 100),
                new (60, 1, .3m, 100),
                new (65, 2, 2, 90)
            }
        };
        var clip2 = new Clip(4m, true)
        {
            Notes = new SortedList<NoteEvent>
            {
                new (36, 0, .25m, 100),
                new (48, .5m, .15m, 100),
                new (60, 1, .3m, 100),
                new (65, 2, 2, 90)
            }
        };
        Assert.AreEqual(clip, clip2);
        var clip3 = new Clip(clip2);
        clip3.Notes[0].Pitch = 40;
        Assert.AreEqual(clip2.Notes[0].Pitch, 36);
        
        var note1 = new NewNoteEvent(36, 0, .25m, 100);
        var note2 = new NewNoteEvent(36, 0, .25m, 100);
        // var note2 = note1 with { Pitch = 40 };
        Assert.AreEqual(note1, note2);
    }
    
    [Test]
    public void TestGroupSimultaneousNotesAndFlattenNotes()
    {
        var clip = new Clip(4, true)
        {
            Notes = new SortedList<NoteEvent>()
            {
                new NoteEvent(60, 0, 2m, 100),
                new NoteEvent(62, .5m, .5m, 100),
                new NoteEvent(62, 1, .5m, 100),
                new NoteEvent(65, 1, 1m, 100),
                new NoteEvent(60, 2, .5m, 100),
                new NoteEvent(62, 2.5m, .5m, 100),
                new NoteEvent(66, 2.5m, .5m, 100)
            }
        };
        clip.GroupSimultaneousNotes();

        Assert.AreEqual(clip.Notes[0].Start, 0);
        Assert.AreEqual(clip.Notes[1].Start, .5m);
        Assert.AreEqual(clip.Notes[2].Start, 1);
        Assert.AreEqual(clip.Notes[3].Start, 2);
        Assert.AreEqual(clip.Notes[4].Start, 2.5m);
            
        clip.Flatten();

        Assert.AreEqual(clip.Notes[0].Start, 0);
        Assert.AreEqual(clip.Notes[1].Start, .5m);
        Assert.AreEqual(clip.Notes[2].Start, 1);
        Assert.AreEqual(clip.Notes[3].Start, 1);
        Assert.AreEqual(clip.Notes[4].Start, 2);
        Assert.AreEqual(clip.Notes[5].Start, 2.5m);
        Assert.AreEqual(clip.Notes[6].Start, 2.5m);
    }
    
    [Test]
    public void TestSortedList()
    {
        var sortedList = new SortedList<NoteEvent>();
        sortedList.Add(new NoteEvent(64, 1, 1, 100));
        sortedList.Add(new NoteEvent(60, 0, 1, 100));
        sortedList.Add(new NoteEvent(60, .5m, 1, 100));
        sortedList.Add(new NoteEvent(60, .1m, 1, 100));
        sortedList.Add(new NoteEvent(60, 2.00001m, 1, 100));
        sortedList.Add(new NoteEvent(60, 7, 1, 100));
        sortedList.Add(new NoteEvent(60, 12.5m, 1, 100));
        sortedList.Add(new NoteEvent(60, 10, 1, 100));
        sortedList.Add(new NoteEvent(60, 2, 1, 100));
        sortedList.Add(new NoteEvent(60, 1, 1, 100));

        Assert.AreEqual(sortedList[0].Start, 0);
        Assert.AreEqual(sortedList[1].Start, .1m);
        Assert.AreEqual(sortedList[2].Start, .5m);
        Assert.AreEqual(sortedList[3].Start, 1);
        Assert.AreEqual(sortedList[3].Pitch, 60);
        Assert.AreEqual(sortedList[4].Start, 1);
        Assert.AreEqual(sortedList[4].Pitch, 64);
        Assert.AreEqual(sortedList[5].Start, 2);
        Assert.AreEqual(sortedList[6].Start, 2.00001m);
        Assert.AreEqual(sortedList[7].Start, 7);
        Assert.AreEqual(sortedList[8].Start, 10);
        Assert.AreEqual(sortedList[9].Start, 12.5m);
    }

    [Test]
    public void TestDuplicateEntries()
    {
        var list = new SortedList<NoteEvent>();
        var noteEvent = new NoteEvent(64, 1, 1, 100);
        list.Add(noteEvent);
        list.Add(noteEvent with {});
        Assert.AreEqual(list.Count, 1);
    }
}