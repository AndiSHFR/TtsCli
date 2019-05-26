namespace TtsCli
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Speech.Synthesis;
    using System.Speech.AudioFormat;
    using NAudio.Wave;
    using NAudio.Lame;

    class Program
    {
        /// <summary>
        /// Exit codes to be returned to the calling application
        /// </summary>
        public enum ExitCodes : int
        {
            /// <summary>No error</summary>
            Success = 0,
            /// <summary>An application error occured</summary>
            ApplicationError = -1,
            /// <summary>While parsing the command line an error occurred</summary>
            OptionParseError = -2,
        }

        #region Declare variables width default settings
        /// <summary>Application exit code to return to the caller</summary>
        static ExitCodes exitCode = ExitCodes.Success;

        /// <summary> Smaples per second for output audio format</summary>
        static int samplesPerSecond = 16025;
        /// <summary>Bits per sample for output audio format</summary>
        static AudioBitsPerSample bitsPerSample = AudioBitsPerSample.Sixteen;
        /// <summary>Number of audio channels for output audio format</summary>
        static AudioChannel audioChannel = AudioChannel.Mono;

        /// <summary>If set to TRUE a brief help and description will be shown</summary>
        static bool showHelp = false;

        /// <summary>If set to TRUE a list of installed voice names will be shown</summary>
        static bool listVoices = false;

        /// <summary>Name of the voice to use when creating the speech output. An empty string is the default voice.</summary>
        static string voiceName = String.Empty;
        /// <summary>Output volume of the speech. 0 is muted, 100 is loud</summary>
        static int speechLoudness = 80;
        /// <summary>The rate at wich the spoken appear. Valid range is from -10 (slow) to 10 (fast)</summary>
        static int speechRate = 0;
        /// <summary>The date and time format to use when replacing the well known tokens {NOW}, {DATE} and {TIME}</summary>
        static string dateTimeFormat = String.Empty;
        /// <summary>Name of the output file when saving the audio data to a file. Valid extensions are .wav and .mp3</summary>
        static string outputFilename = String.Empty;
        /// <summary>Text to convert to speech</summary>
        static string text = String.Empty;
        #endregion

        /// <summary> ... </summary>
        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptions]
        static void Main(string[] args)
        {

            try
            {
                string parseError = String.Empty;

                
                #region Commandline parsing
                /**
                 * To process the command line arguments we use the Mono.Options class Options.
                 * Caution: The : (optional) and = (required) specifier for an option define
                 *          if a parmeter for this specific option is either optional or required.
                 *          It does _not_ define if the option itself is optional or required.
                 */
                var optionSet = new Mono.Options.OptionSet() {
                    { "i|info", "List available voice names", (string v) => listVoices = (null!=v) },
                    { "v=|voice=", "Name of the voice to create the speech output", (string value) => voiceName = value },
                    { "l=|loudness=", "Volume of the output. Range is 0 (muted) to 100 (loud). Default is 80", (int value) => speechLoudness = value },
                    { "s=|speed=", "Speech rate. Range is form -10 to 10. Default is 0", (int value) => speechRate = value },
                    { "d=|dateformat=", "Date format string. i.e. \"\"", (string value) => dateTimeFormat = value },
                    { "o=|output=", "Output wave filename (.wav)", (string value) => outputFilename = value },
                    { "h|?|help", "Show help message", (string v) => showHelp = (null!=v) }
                 };

                // Try to parse the command line according to the option set.
                try
                {
                    List<string> extraOptions = optionSet.Parse(args);
                    if (extraOptions.Count > 0)
                    {
                        text = extraOptions[0];
                    }
                }
                catch (Mono.Options.OptionException ex)
                {
                    // Keep parser error for later reference
                    parseError = ex.Message;
                }
                #endregion

                // No parse error and no text to convert, well thats odd!
                if (String.IsNullOrEmpty(parseError) && String.IsNullOrEmpty(text))
                {
                    parseError = "Text is missing! Please specify the text you want to convert to speech. See help (--h) for further options and information.";
                }

                // Any parse error? So tell the user about it.
                if (!String.IsNullOrEmpty(parseError))
                {
                    exitCode = ExitCodes.OptionParseError;
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Error: {0}", parseError);
                }
                else

                // Need to show a brief help?
                if (showHelp)
                {

                    #region AssemblyInfos
                    Dictionary<string, string> assemblyInfos = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    object[] customAttributes = typeof(Program).Assembly.GetCustomAttributes(false);
                    foreach (object attribute in customAttributes)
                    {

                        if (attribute.GetType() == typeof(AssemblyTitleAttribute))
                        {
                            assemblyInfos["Title"] = ((AssemblyTitleAttribute)attribute).Title;
                        }

                        if (attribute.GetType() == typeof(AssemblyProductAttribute))
                        {
                            assemblyInfos["Product"] = ((AssemblyProductAttribute)attribute).Product;
                        }

                        if (attribute.GetType() == typeof(AssemblyCompanyAttribute))
                        {
                            assemblyInfos["Company"] = ((AssemblyCompanyAttribute)attribute).Company;
                        }

                        if (attribute.GetType() == typeof(AssemblyCopyrightAttribute))
                        {
                            assemblyInfos["Copyright"] = ((AssemblyCopyrightAttribute)attribute).Copyright;
                        }

                        if (attribute.GetType() == typeof(AssemblyDescriptionAttribute))
                        {
                            assemblyInfos["Description"] = ((AssemblyDescriptionAttribute)attribute).Description;
                        }

                        if (attribute.GetType() == typeof(AssemblyFileVersionAttribute))
                        {
                            assemblyInfos["FileVersion"] = ((AssemblyFileVersionAttribute)attribute).Version;
                        }

                    }
                    #endregion

                    Console.Out.WriteLine(string.Format(
                      "{0} Version {1}\r\n{2}",
                      assemblyInfos["Description"], Assembly.GetExecutingAssembly().GetName().Version,
                      assemblyInfos["Copyright"]
                    ));

                    Console.Out.WriteLine();
                    Console.Out.WriteLine("This commandline application will create speech output from a text.");
                    Console.Out.WriteLine();
                    Console.Out.WriteLine(string.Format("Usage: {0}.exe [-h|-?|--help] [-l|--loudness <0..100>] [-s|--speed <1|2|3>] \"A text to be spoken.\"", assemblyInfos["Title"]));
                    Console.Out.WriteLine(string.Format("Example: {0}.exe \"Hello, world!\"", assemblyInfos["Title"]));
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Options:");
                    optionSet.WriteOptionDescriptions(Console.Out);

                }
                else
                {

                    using (var speechSynthesizer = new SpeechSynthesizer())
                    {

                        // Do we have to show a list of available voices installed?
                        if (listVoices)
                        {
                            Console.Out.WriteLine("Available voice names:");
                            foreach (var v in speechSynthesizer.GetInstalledVoices())
                            {
                                Console.Out.Write("\t");
                                Console.Out.WriteLine(v.VoiceInfo.Name);
                            }
                        }
                        else
                        {
                            // Limit input values to valid range
                            speechLoudness = Math.Min(Math.Max(speechLoudness, 0), 100);
                            speechRate = Math.Min(Math.Max(speechRate, -10), 10);

                            // Replace well know tokens in the text
                            text = Regex.Replace(text, "{NOW}", DateTime.Now.ToString(String.Empty), RegexOptions.IgnoreCase);
                            text = Regex.Replace(text, "{DATE}", DateTime.Now.Date.ToString(String.Empty), RegexOptions.IgnoreCase);
                            text = Regex.Replace(text, "{TIME}", DateTime.Now.TimeOfDay.ToString(String.Empty), RegexOptions.IgnoreCase);
                            text = Regex.Replace(text, "{COMPUTERNAME}", Environment.MachineName, RegexOptions.IgnoreCase);

                            // Select a default voice we want to use
                            speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);

                            // If the user specified a special voice to use, select it now.
                            if (!String.IsNullOrEmpty(voiceName)) speechSynthesizer.SelectVoice(voiceName);

                            // Set volume and rate
                            speechSynthesizer.Volume = speechLoudness;
                            speechSynthesizer.Rate = speechRate;

                            // Create a memory stream. From this stream we will output the audio data
                            using (MemoryStream streamAudio = new MemoryStream())
                            {
                                // Configure the synthesizer to output to an audio stream.  
                                // Because we need to set the audio format too we have to 
                                // use some reflection here to get the non-pulic "SetOutputStream" method.
                                var setOutputStream = speechSynthesizer.GetType().GetMethod("SetOutputStream", BindingFlags.Instance | BindingFlags.NonPublic);
                                var audioFormatInfo = new SpeechAudioFormatInfo(samplesPerSecond, bitsPerSample, audioChannel);
                                setOutputStream.Invoke(speechSynthesizer, new object[] { streamAudio, audioFormatInfo, true, true });

                                // Now "speak" the text to the output stream
                                speechSynthesizer.Speak(text);

                                // Reset the output streams position back to the beginning.
                                streamAudio.Position = 0;

                                // Do we have an output filename?
                                if (!String.IsNullOrEmpty(outputFilename))
                                {
                                    // Get the file extension to determine the output file format
                                    var fileExtension = Path.GetExtension(outputFilename);

                                    // Do we have to output a mp3 file?
                                    if (0 == String.Compare(".MP3", fileExtension, true))
                                    {
                                        using (var waveReader = new WaveFileReader(streamAudio))
                                        using (var mp3Writer = new LameMP3FileWriter(outputFilename, waveReader.WaveFormat, LAMEPreset.VBR_90))
                                        {
                                            waveReader.CopyTo(mp3Writer);
                                        }

                                    }
                                    else

                                    // Do we have to output a wave file?
                                    if (0 == String.Compare(".WAV", fileExtension, true))
                                    {
                                        using (FileStream file = new FileStream(outputFilename, FileMode.Create, System.IO.FileAccess.Write))
                                        {
                                            streamAudio.CopyTo(file);
                                        }
                                    }
                                    else
                                        // Neither mp3 nor wav, so we do not know how to handle the output
                                        throw new ApplicationException(string.Format("Invalid output file format \"{0}\". Either .WAV or .MP3 allowed!", fileExtension));

                                }
                                else
                                {
                                    // No output to a file so we will output the stream to the default audio device.
                                    using (var soundPlayer = new System.Media.SoundPlayer(streamAudio))
                                    {
                                        soundPlayer.PlaySync();
                                    }
                                }

                                // Set the synthesizer output to null to release the stream.   
                                speechSynthesizer.SetOutputToNull();

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                exitCode = ExitCodes.ApplicationError;
            }
            Environment.ExitCode = (int)exitCode;
            return;
        }
    }
}

