using OsuParsers.Beatmaps;
using System.Xml;
using System.IO;
using OsuParsers.Beatmaps.Objects;
using OsuParsers;
using System.Collections;
using OsuParsers.Enums.Beatmaps;
using OsuParsers.Beatmaps.Sections;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Drawing;

//https://github.com/mrflashstudio/OsuParsers#usage
class Program
{
    static List<Tuple<int, int>> holdNotesTimeSpans = new();
    public static List<HitSoundType> parseEdgeHitSounds(string item)
    {
        var hitsoundsStr = item.Split("|");

        List<HitSoundType> toReturn = new();
        foreach(var hs in hitsoundsStr)
        {
            toReturn.Add(getHitsound(Convert.ToInt32(hs)));
        }

        return toReturn;
    }

    private static ArrayList SplitAttributes(string item)
    {
        string[] items = item.Split('%');
        ArrayList toReturn = new();


        for (int i=0; i<items.Length; ++i)
        {
            int intItem = 0;
            bool boolItem = false;
            float floatItem = 0.0f;

            if (int.TryParse(items[i], out intItem))
                toReturn.Add(intItem);
            else if (bool.TryParse(items[i], out boolItem))
                toReturn.Add(boolItem);
            else if (float.TryParse(items[i], System.Globalization.NumberStyles.Any, new NumberFormatInfo() { NumberDecimalSeparator = "." }, out floatItem))
                toReturn.Add(floatItem);
            else
                toReturn.Add(items[i]);
        }
        return toReturn;
    }

    public static BeatmapDifficultySection convertDifficultySection(string _string)
    {
        ArrayList items = SplitAttributes(_string);
        BeatmapDifficultySection section = new BeatmapDifficultySection();

        section.HPDrainRate = (int)items[0];
        section.CircleSize = (int)items[1];
        section.OverallDifficulty = (int)items[2];
        section.ApproachRate = (int)items[3];
        section.SliderMultiplier = Convert.ToSingle(items[4]) / 2;
        section.SliderTickRate = (int)items[5];


        return section;
    }


    public static BeatmapEventsSection convertEventSection(string _string)
    {
        ArrayList items = SplitAttributes(_string);

        BeatmapEventsSection section = new BeatmapEventsSection();

        //only breaks
        for(int i=0; i<items.Count; i+=2)
        {
            section.Breaks.Add(new OsuParsers.Beatmaps.Sections.Events.BeatmapBreakEvent((int)items[i], (int)items[i + 1]));
            //TODO: FIX SHIT THIS IF POSSIBLE
        }

        return section;
    }

    public static BeatmapGeneralSection convertGeneralSection(string _string)
    {
        ArrayList items = SplitAttributes(_string);


        BeatmapGeneralSection section = new BeatmapGeneralSection();

        //section.AudioFilename = (string)items[0];
        section.AudioFilename = "audio.mp3";
        section.AudioLeadIn = (int)items[1];
        section.PreviewTime = (int)items[2];
        section.Countdown = (int)items[3] == 0? false : true;

        if (items[4] == "normal")
            section.SampleSet = SampleSet.Normal;
        if (items[4] == "soft")
            section.SampleSet = SampleSet.Soft;

        section.StackLeniency = 0.2;
        //section.StackLeniency = (double)(float)items[5];

        section.Mode = OsuParsers.Enums.Ruleset.Standard;

        section.LetterboxInBreaks = (bool)items[7];


        return section;
    }

    private static BeatmapMetadataSection convertMetaDataSection(string line)
    {
        ArrayList items = SplitAttributes(line);


        BeatmapMetadataSection section = new BeatmapMetadataSection();

        section.Title = (string)items[0];

        section.Artist = (string)items[1];

        section.Creator = "LosPedros";

        section.Version = (string)items[3];

        section.Source = (string)items[5];

        section.BeatmapSetID = -1;
        section.BeatmapID = -1;

        section.TagsString = (string)items[2] + " hexis";
        return section;
    }

    private static BeatmapColoursSection convertColourSection(string line)
    {
        ArrayList items = SplitAttributes(line);


        BeatmapColoursSection section = new BeatmapColoursSection();

        for(int i=0; i<items.Count; i+=3)
        {
            section.ComboColours.Add(Color.FromArgb((int)items[i], (int)items[i + 1], (int)items[i + 2]));
            //TODO FIX 
        }
        return section;
    }

    public static Tuple<TimingPoint, TimingPoint?> convertTimingPoint(string _string) {
        ArrayList items = SplitAttributes(_string);
        TimingPoint timingPoint = new TimingPoint();
        timingPoint.Inherited = (bool)items[0];

        TimingPoint additionalAfterNonInherited = null;

        if (timingPoint.Inherited == false)
        {
            timingPoint.Offset = (int)items[1];

            timingPoint.BeatLength = 60000.0f / Convert.ToSingle(items[2]);

            if ((int)items[3] == 4)
                timingPoint.TimeSignature = OsuParsers.Enums.Beatmaps.TimeSignature.SimpleQuadruple;
            if ((int)items[3] == 3)
                timingPoint.TimeSignature = OsuParsers.Enums.Beatmaps.TimeSignature.SimpleTriple;

            if ((int)items[4] == 1)
                timingPoint.SampleSet = OsuParsers.Enums.Beatmaps.SampleSet.Normal;
            if ((int)items[4] == 2)
                timingPoint.SampleSet = OsuParsers.Enums.Beatmaps.SampleSet.Soft;

            timingPoint.CustomSampleSet = (int)items[5];

            timingPoint.Volume = (int)items[6];

            if ((bool)items[7])
                timingPoint.Effects = OsuParsers.Enums.Beatmaps.Effects.Kiai;
            else
                timingPoint.Effects = OsuParsers.Enums.Beatmaps.Effects.None;







            additionalAfterNonInherited = new();
            additionalAfterNonInherited.Inherited = true;

            additionalAfterNonInherited.BeatLength = 100.0f / -1 / 2;

            additionalAfterNonInherited.Offset = (int)items[1];

            if ((int)items[3] == 4)
                additionalAfterNonInherited.TimeSignature = OsuParsers.Enums.Beatmaps.TimeSignature.SimpleQuadruple;
            if ((int)items[3] == 3)
                additionalAfterNonInherited.TimeSignature = OsuParsers.Enums.Beatmaps.TimeSignature.SimpleTriple;

            if ((int)items[4] == 1)
                additionalAfterNonInherited.SampleSet = OsuParsers.Enums.Beatmaps.SampleSet.Normal;
            if ((int)items[4] == 2)
                additionalAfterNonInherited.SampleSet = OsuParsers.Enums.Beatmaps.SampleSet.Soft;

            additionalAfterNonInherited.CustomSampleSet = (int)items[5];

            additionalAfterNonInherited.Volume = (int)items[6];

            if ((bool)items[7])
                additionalAfterNonInherited.Effects = OsuParsers.Enums.Beatmaps.Effects.Kiai;
            else
                additionalAfterNonInherited.Effects = OsuParsers.Enums.Beatmaps.Effects.None;
        }
        else
        {
            timingPoint.BeatLength = 100.0f / -Convert.ToSingle(items[2]) / 2;

            timingPoint.Offset = (int)items[1];

            if ((int)items[3] == 4)
                timingPoint.TimeSignature = OsuParsers.Enums.Beatmaps.TimeSignature.SimpleQuadruple;
            if ((int)items[3] == 3)
                timingPoint.TimeSignature = OsuParsers.Enums.Beatmaps.TimeSignature.SimpleTriple;

            if ((int)items[3] == 1)
                timingPoint.SampleSet = OsuParsers.Enums.Beatmaps.SampleSet.Normal;
            if ((int)items[3] == 2)
                timingPoint.SampleSet = OsuParsers.Enums.Beatmaps.SampleSet.Soft;

            timingPoint.CustomSampleSet = (int)items[4];

            timingPoint.Volume = (int)items[5];

            if ((bool)items[6])
                timingPoint.Effects = OsuParsers.Enums.Beatmaps.Effects.Kiai;
            else
                timingPoint.Effects = OsuParsers.Enums.Beatmaps.Effects.None;
        }
        return new Tuple<TimingPoint, TimingPoint?>(timingPoint, additionalAfterNonInherited);
    }

    public static HitSoundType getHitsound(int val)
    {
        if (val == 2)
            return HitSoundType.Whistle;
        if (val == 4)
            return HitSoundType.Finish;
        if (val == 8)
            return HitSoundType.Clap;

        if (val == 6)
            return HitSoundType.Finish | HitSoundType.Whistle;
        if (val == 12)
            return HitSoundType.Finish | HitSoundType.Clap;
        if (val == 10)
            return HitSoundType.Whistle | HitSoundType.Clap;
        if (val == 16)
            return HitSoundType.Whistle | HitSoundType.Clap | HitSoundType.Finish;

        return HitSoundType.None;
    }

    public static HitCircle convertHitCircle(string _string)
    {
        ArrayList items = SplitAttributes(_string);

        HitCircle hitCircle = new(
            new System.Numerics.Vector2(Convert.ToInt32(items[2]), Convert.ToInt32(items[3])),
            (int)items[1],
            (int)items[1],
            getHitsound((int)items[5]),
            null,
            (bool)items[4],
            0
            );


        return hitCircle;
    }
    public static Spinner convertSpinner(string _string)
    {
        ArrayList items = SplitAttributes(_string);



        Spinner spinner = new(
            new Vector2(256, 192),
            (int)items[1],
            (int)items[6],
            getHitsound((int)items[5]),
            null,
            (bool)items[4],
            0
            );


        return spinner;
    }

    public static Slider convertSlider(string _string, string _string2)
    {
        ArrayList items = SplitAttributes(_string);

        ArrayList pointsList = SplitAttributes(_string2);

        var curveType = CurveType.Linear;
        if(pointsList.Count == 5)
        {
            curveType = CurveType.PerfectCurve;
        }
        else if(pointsList.Count > 5)
        {
            curveType = CurveType.Bezier;
        }

        List<Vector2> points = new();

        for(int i=0; i< pointsList.Count; i+=2)
        {
            int asd = 0;
            if(int.TryParse(pointsList[i].ToString(), out asd)  && int.TryParse(pointsList[i+1].ToString(), out asd))
                points.Add(new Vector2((int)pointsList[i], (int)pointsList[i+1]));
        }
        Slider slider = null;
        if(items.Count > 9)
        {
            slider = new(new System.Numerics.Vector2((int)items[2], (int)items[3]),
            (int)items[1],
            0,
            getHitsound((int)items[5]),
            curveType,
            points,
            (int)items[8] + 1,
            (int)items[7],
            (bool)items[4],
            0,
            parseEdgeHitSounds((string)items[9]),
            null,
            null
            );
        }
        else
        {
            slider = new(new System.Numerics.Vector2((int)items[2], (int)items[3]),
            (int)items[1],
            0,
            getHitsound((int)items[5]),
            curveType,
            points,
            (int)items[8] + 1,
            (int)items[7],
            (bool)items[4],
            0,
            null,
            null,
            null
            );
        }
        
        

        return slider;
    }

    public static List<Vector2> getHoldCircle(double rotations, Vector2 headPosition)
    {
        List<Vector2> points = new();

        for(double i=0; i<rotations; ++i) {
            points.Add(new Vector2(headPosition.X, headPosition.Y + 4));
            points.Add(new Vector2(headPosition.X, headPosition.Y + 4));

            points.Add(new Vector2(headPosition.X - 3, headPosition.Y + 3));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y + 3));

            points.Add(new Vector2(headPosition.X -4, headPosition.Y));
            points.Add(new Vector2(headPosition.X -4, headPosition.Y));

            points.Add(new Vector2(headPosition.X - 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y - 3));

            points.Add(new Vector2(headPosition.X, headPosition.Y - 4));
            points.Add(new Vector2(headPosition.X, headPosition.Y - 4));


            points.Add(new Vector2(headPosition.X + 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X + 3, headPosition.Y - 3));

            points.Add(new Vector2(headPosition.X + 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X + 4, headPosition.Y));

            points.Add(new Vector2(headPosition.X + 3, headPosition.Y + 3));
            points.Add(new Vector2(headPosition.X + 3, headPosition.Y + 3));
        }
        points.Add(new Vector2(headPosition.X, headPosition.Y + 4));
        points.Add(new Vector2(headPosition.X, headPosition.Y + 4));


        //Console.WriteLine(rotations % 1);
        if (rotations % 1 >= (0.125 * 1))
        {
            points.Add(new Vector2(headPosition.X, headPosition.Y + 4));
            points.Add(new Vector2(headPosition.X, headPosition.Y + 4));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y + 3));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y + 3));
        }
        if (rotations % 1 >= (0.125 * 2))
        {
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y + 3));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y + 3));
            points.Add(new Vector2(headPosition.X - 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X - 4, headPosition.Y));
        }
        if (rotations % 1 >= (0.125 * 3))
        {
            points.Add(new Vector2(headPosition.X - 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X - 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y - 3));
        }
        if (rotations % 1 >= (0.125 * 4))
        {
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X - 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X, headPosition.Y - 4));
            points.Add(new Vector2(headPosition.X, headPosition.Y - 4));

        }
        if (rotations % 1 >= (0.125 * 5))
        {
            points.Add(new Vector2(headPosition.X, headPosition.Y - 4));
            points.Add(new Vector2(headPosition.X, headPosition.Y - 4));
            points.Add(new Vector2(headPosition.X + 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X + 3, headPosition.Y - 3));
        }
        if (rotations % 1 >= (0.125 * 6))
        {
            points.Add(new Vector2(headPosition.X + 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X + 3, headPosition.Y - 3));
            points.Add(new Vector2(headPosition.X + 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X + 4, headPosition.Y));

        }
        if (rotations % 1 >= (0.125 * 7))
        {
            points.Add(new Vector2(headPosition.X + 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X + 4, headPosition.Y));
            points.Add(new Vector2(headPosition.X, headPosition.Y + 4));
            points.Add(new Vector2(headPosition.X, headPosition.Y + 4));
        }




        points.Add(headPosition);

       


        return points;


    }

    public static Tuple<Slider, TimingPoint, TimingPoint?, TimingPoint> convertHoldNote(string _string, Beatmap beatmap, double timeForBeat, double sliderMultiplier)
    {
        ArrayList items = SplitAttributes(_string);


        var headPos = new Vector2((int)items[2], (int)items[3]);

        double calcRotations = Convert.ToDouble(((int)items[6] - (int)items[1])) / timeForBeat;
        calcRotations = Math.Round(calcRotations, 2);

        double trueSliderLength = ((int)items[6] - (int)items[1]) / timeForBeat;
        int rotations = 0;

        rotations = Convert.ToInt32(calcRotations);
        bool calculateFromTrueSliderLength = false;
        if (calcRotations < 1) {
            calculateFromTrueSliderLength = true;
            rotations = 1;
        }

        //last used timing point
        TimingPoint tp = null;
        // Console.WriteLine((int)items[1]);
        foreach (var tpoint in beatmap.TimingPoints)
        {
            if (tpoint.Offset <= (int)items[1])
            {
                
                tp = tpoint;
            }
        }
        if(tp == null)
        {
            Console.WriteLine("Error occured while converting hold note. Missing timing point before holdnote.");
            throw new Exception("Cannot convert holdnote");
        }


        List<Vector2> points = getHoldCircle(calcRotations, headPos);
        double length = 0;
        for (int i = 0; i < calcRotations; ++i) {
            length += 25.298221281347;
        }

        length += (calcRotations % 1) * 25.298221281347;

        length += 8;


        TimingPoint svchange = new();

        svchange.Offset = (int)items[1] + 1; ////////////////////////////////////////////////////////////////////// 0.5
        svchange.Inherited = true;
        svchange.TimeSignature = tp.TimeSignature;
        svchange.SampleSet = tp.SampleSet;
        svchange.CustomSampleSet = tp.CustomSampleSet;
        svchange.Volume = tp.Volume;
        svchange.Effects = tp.Effects;

        svchange.BeatLength = -(10000 / length) * sliderMultiplier * calcRotations;

        /*if(calculateFromTrueSliderLength)
        {
            svchange.BeatLength = - (((10000 / length) * sliderMultiplier) * trueSliderLength);
        }*/

        Slider holdNote = null;
        if(items.Count > 8)
        {
            holdNote = new Slider(
            headPos,
            (int)items[1],
            0,
            getHitsound((int)items[5]),
            CurveType.Bezier,
            points,
            1,
            length,
            (bool)items[4],
            0,
            parseEdgeHitSounds((string)items[8])
            );
        }
        else
        {
            holdNote = new Slider(
            headPos,
            (int)items[1],
            0,
            getHitsound((int)items[5]),
            CurveType.Bezier,
            points,
            1,
            length,
            (bool)items[4],
            0
            );
        }

        


        TimingPoint endTimingPoint = new();
        endTimingPoint.Offset = (int)items[1] + Convert.ToInt32(timeForBeat * trueSliderLength);
        endTimingPoint.Inherited = true;
        endTimingPoint.TimeSignature = tp.TimeSignature;
        endTimingPoint.SampleSet = tp.SampleSet;
        endTimingPoint.CustomSampleSet = tp.CustomSampleSet;
        endTimingPoint.Volume = tp.Volume;
        endTimingPoint.Effects = tp.Effects;

        endTimingPoint.BeatLength = tp.BeatLength;


        TimingPoint tickrateTimingPoint = new();
        tickrateTimingPoint.Offset = (int)items[1];
        tickrateTimingPoint.Inherited = true;
        tickrateTimingPoint.TimeSignature = tp.TimeSignature;
        tickrateTimingPoint.SampleSet = tp.SampleSet;
        tickrateTimingPoint.CustomSampleSet = tp.CustomSampleSet;
        tickrateTimingPoint.Volume = tp.Volume;
        tickrateTimingPoint.Effects = tp.Effects;

        int tickrate = (int)items[7];

        tickrateTimingPoint.BeatLength = -(10000 / length) * sliderMultiplier * tickrate * rotations / beatmap.DifficultySection.SliderTickRate;

        if (calculateFromTrueSliderLength)
        {
            tickrateTimingPoint.BeatLength = -(((10000 / length) * sliderMultiplier) * trueSliderLength * tickrate ) / beatmap.DifficultySection.SliderTickRate;
        }
        holdNotesTimeSpans.Add(new Tuple<int, int>(tickrateTimingPoint.Offset, endTimingPoint.Offset));

        return new Tuple<Slider, TimingPoint, TimingPoint?, TimingPoint>(holdNote, svchange, endTimingPoint, tickrateTimingPoint);
    }
   
    public static void Transform(string hexisPath, string beatmapPath)
    {
        XmlTextReader reader = null;

        try
        {
            reader = new XmlTextReader(hexisPath);
            reader.WhitespaceHandling = WhitespaceHandling.None;

            Beatmap beatmapToExport = new Beatmap();

            while (reader.Read())
            {

                if (reader.HasAttributes)
                {
                    if (reader.Name == "timing-point")
                    {
                        String line = "";
                        for (int i = 0; i < reader.AttributeCount; ++i)
                        {
                            line += reader.GetAttribute(i);
                            if (i < reader.AttributeCount - 1)
                                line += "%";
                        }
                        Tuple<TimingPoint, TimingPoint?> tps = convertTimingPoint(line);
                        beatmapToExport.TimingPoints.Add(tps.Item1);
                        if (tps.Item2 != null)
                            beatmapToExport.TimingPoints.Add(tps.Item2);
                    }

                    if (reader.Name == "general")
                    {
                        String line = "";
                        for (int i = 0; i < reader.AttributeCount; ++i)
                        {
                            line += reader.GetAttribute(i);
                            if (i < reader.AttributeCount - 1)
                                line += "%";
                        }

                        beatmapToExport.GeneralSection = convertGeneralSection(line);
                    }

                    if (reader.Name == "meta")
                    {
                        String line = "";
                        for (int i = 0; i < reader.AttributeCount; ++i)
                        {
                            line += reader.GetAttribute(i);
                            if (i < reader.AttributeCount - 1)
                                line += "%";
                        }

                        beatmapToExport.MetadataSection = convertMetaDataSection(line);
                    }

                    if (reader.Name == "difficulty")
                    {
                        String line = "";
                        for (int i = 0; i < reader.AttributeCount; ++i)
                        {
                            line += reader.GetAttribute(i);
                            if (i < reader.AttributeCount - 1)
                                line += "%";
                        }

                        beatmapToExport.DifficultySection = convertDifficultySection(line);
                    }

                    if (reader.Name == "hit-object")
                    {
                        int type = -1;
                        if (!int.TryParse(reader.GetAttribute(0), out type))
                            throw new Exception("Something went wrong with parsing hit-object.");

                        if (type == 1) // HitCircle
                        {
                            String line = "";
                            for (int i = 0; i < reader.AttributeCount; ++i)
                            {
                                line += reader.GetAttribute(i);
                                if (i < reader.AttributeCount - 1)
                                    line += "%";
                            }
                            HitCircle cs = convertHitCircle(line);

                            beatmapToExport.HitObjects.Add(cs);

                        }


                        if (type == 2) // Slider
                        {
                            string line = "";
                            for (int i = 0; i < reader.AttributeCount; ++i)
                            {
                                line += reader.GetAttribute(i);
                                if (i < reader.AttributeCount - 1)
                                    line += "%";
                            }

                            int pointCount = 0;


                            reader.Read();
                            if (reader.Name == "hit-sound")
                                continue;

                            string line2 = "";
                            while (reader.Name == "point")
                            {
                                pointCount++;
                                line2 += reader.GetAttribute(0) + "%";
                                line2 += reader.GetAttribute(1) + "%";

                                reader.Read();
                            }

                            Slider slider = convertSlider(line, line2); // bardziej sliderhead

                            //tera iterujesz po nastepnych reader.Read() 
                            //  lapiesz skurwysynów w liste
                            // i patrzysz ile ich jest (discord pedros)

                            beatmapToExport.HitObjects.Add(slider);
                        }

                        if (type == 4) // Spinner
                        {
                            string line = "";
                            for (int i = 0; i < reader.AttributeCount; ++i)
                            {
                                line += reader.GetAttribute(i);
                                if (i < reader.AttributeCount - 1)
                                    line += "%";
                            }

                            Spinner spinner = convertSpinner(line);
                            beatmapToExport.HitObjects.Add(spinner);

                        }

                        if (type == 8) // hold note = slider
                        {
                            string line = "";
                            for (int i = 0; i < reader.AttributeCount; ++i)
                            {
                                line += reader.GetAttribute(i);
                                if (i < reader.AttributeCount - 1)
                                    line += "%";
                            }
                            //TimingPoint ostatnioUzyty = beatmapToExport.TimingPoints[beatmapToExport.TimingPoints.Count - 1];

                            double timeForBeat = 0;
                            foreach (var tp in beatmapToExport.TimingPoints)
                            {
                                if (!tp.Inherited)
                                {
                                    timeForBeat = tp.BeatLength;
                                }
                            }

                            Tuple<Slider, TimingPoint, TimingPoint?, TimingPoint> stuff = convertHoldNote(line, beatmapToExport, timeForBeat, beatmapToExport.DifficultySection.SliderMultiplier);

                            beatmapToExport.TimingPoints.Add(stuff.Item4);
                            beatmapToExport.HitObjects.Add(stuff.Item1);
                            if(stuff.Item2 != null)
                            {
                                //Console.WriteLine(stuff.Item2.Offset + " " + stuff.Item2.BeatLength);
                                beatmapToExport.TimingPoints.Add(stuff.Item2);
                            }

                            beatmapToExport.TimingPoints.Add(stuff.Item3);


                        }
                    }


                    if (reader.Name == "break")
                    {
                        String line = "";
                        for (int i = 0; i < reader.AttributeCount; ++i)
                        {
                            line += reader.GetAttribute(i);
                            if (i < reader.AttributeCount - 1)
                                line += "%";
                        }

                        beatmapToExport.EventsSection = convertEventSection(line);
                    }

                    if (reader.Name == "combo")
                    {
                        String line = "";
                        for (int i = 0; i < reader.AttributeCount; ++i)
                        {
                            line += reader.GetAttribute(i);
                            if (i < reader.AttributeCount - 1)
                                line += "%";
                        }

                        beatmapToExport.ColoursSection = convertColourSection(line);
                    }



                }
            }

            beatmapToExport.EditorSection.DistanceSpacing = 1;
            beatmapToExport.EditorSection.BeatDivisor = 4;
            beatmapToExport.EditorSection.GridSize = 4;
            beatmapToExport.EditorSection.TimelineZoom = 2;




            beatmapToExport.Save(beatmapPath);

            FixTimingPointsByPoint5(beatmapPath);

        }
        finally
        {
            if (reader != null)
                reader.Close();
        }

    }


    public static void Main(string[] args)
    {
        var files = Directory.GetFiles("toConvert");

        foreach (var file in files)
        {
            holdNotesTimeSpans.Clear();
            Transform(file, "converted/" + Path.GetFileNameWithoutExtension(file) + ".osu");
            File.WriteAllText("converted/" + Path.GetFileNameWithoutExtension(file) + ".hnts", holdNotesTimeStampToString());
        }
    }
    public static string holdNotesTimeStampToString()
    {
        string outputString = "";

        foreach (var ts in holdNotesTimeSpans) {
            outputString += ts.Item1 + ", " + ts.Item2 + "\n";
        }

        return outputString;
    }
    public static void FixTimingPointsByPoint5(string beatmapPath)
    {
        FileStream file = File.Open(beatmapPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamReader sr = new StreamReader(file);


        string lineOfText = "";
        string oldLineOfText = "";

        bool start_reading = false;

        List<Tuple<string, string>> listOfNewTimingPoints = new();
        while ((lineOfText = sr.ReadLine()) != null)
        {
            if (lineOfText.Length < 5 && start_reading)
                break;



            if (start_reading)
            {

                if (oldLineOfText.Contains("[TimingPoints]"))
                {
                    oldLineOfText = lineOfText;
                    continue;
                }

                if (Convert.ToInt32(lineOfText.Split(',')[0].Trim()) == Convert.ToInt32(oldLineOfText.Split(',')[0].Trim()) + 1)
                {
                    string toWrite = lineOfText.Replace(
                        lineOfText.Split(',')[0].Trim(),
                        (Convert.ToInt32(lineOfText.Split(',')[0].Trim()) - 1).ToString() + ".5");


                    listOfNewTimingPoints.Add(new Tuple<string, string>(lineOfText, toWrite));
                }


            }



            
            if (lineOfText.Contains("TimingPoints"))
                start_reading = true;


            oldLineOfText = lineOfText;
        }



        sr.Close();
        file.Close();

        foreach (var tup in listOfNewTimingPoints)
        {
            string file_content = File.ReadAllText(beatmapPath);

            file_content = file_content.Replace(tup.Item1, tup.Item2);

            File.WriteAllText(beatmapPath, file_content);

        }

    }
}