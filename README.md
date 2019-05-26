# TtsCli.exe - create audio speech file from text
Command line application to convert text to speech using the windows speech engine.

Here is an example file for the text "Hello, this is your text to speech converter speaking.":

<audio controls="controls">
  <source type="audio/mp3" src="example.mp3"></source>
  <p>Your browser does not support the audio element.</p>
</audio>

Created from the following command line:

```cmd
TtsCli.exe --output ".\example.mp3" "Hello, this is your text to speech converter speaking." 
```

## Requirements

  * Windows OS higher or equal to Windows 7 (Windows 7, Windows 8, Windows 10, Windows Server 2008, Windows Server 2012)


## Command Line Arguments

* __-?|--help__  
Show help and information about command line arguments.

* __-i|--info__  
List available voices.

* __-v|--voice__  
Set the voice used to created the audio output. To see available voices use the __l|list__ command.

* __-l|--loudness__  
The volume of the audio output. Range is from 0 (muted) to 100 (loud).

* __-r|--rate__  
Set the speaking rate of the speech output. Valid values are -10 (very slow) to 10 (very fast). Default is 0.

* __-o|--output__  
Name of the output file to write the audio stream to. If this options is not set the audio stream will be send to the default audio output device. The application can create two output formats. WAV and MP3, depending on the file extension.


## Minimal example
Output the text "Hello, world!" to the default audio output device.

```cmd
TtsCli.exe "Hello, world!"
```

## Sophisticated example
Output the text "Hello, world!" to the the file "hello-world.wav" with the voice "Microsoft Mark Desktop", 100% volume and a sligthly higher speech rate (2). 

```cmd
TtsCli.exe --voice "Microsoft Mark Desktop" --speed 2 --loudness 100 --output ".\hello-world.wav" "Hello, world!"
```

## Error Handling
Errors are written to STDERR.
The application will also set the exit code. If there is no error the exit code will be 0.

