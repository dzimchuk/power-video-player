# Power Video Player

**Project Description**

Power Video Player is a slim feature-rich video/dvd player that meets everyday needs in video playback on PC with a bunch of advanced features on board.

It's written in C# as a WPF application and employs a fair bit of DirectShow, COM Interop and PInvoke programming. Older versions (1.x) were Windows Forms applications and are also available.

![Power Video Player](docs/Home_pvp3d.png)

**Features:**
- Convenient UI
- Plays almost all multimedia formats (requires appropriate codecs to be present on the client system)
- Full DVD Support (MPEG-2 and AC3 decoders should be installed on the client system)  
- Video control: Video size, aspect ratio
- Playback speed control
- Windowed and fullscreen playback
- Displays detailed information about each video and audio stream
- Reports the format that could not be rendered because of the missing codec(s)
- Customizable keyboard and mouse actions  
- Multilingual interface: English, Russian  
- Skinnable (only one default skin is shipped now)
- Supports various video renderers: Legacy Video Renderer, VMR/VMR9, EVR
- Allows to take screenshots of the currently played video
- Drag and Drop support
- Subtitles support (requires DirectVobSub aka VSFilter)
