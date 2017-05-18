# Software Limiter
A small windows service to limit the master volume to XYZ percent. The maximum allowed percentage is time controlled.

## Config file
If not already present, create the directory `C:\ProgramData\SoftwareLimiter`. You need to place a config.txt in this directory. For a start, you could use the one
from this project.

The file format is very simple; each line contains a time followed by a space followed by the maximum allowed volume. For example, to limit all sound between 22:00 and 8:00 to 35%:

```
00:00 35.0
08:00 100.0
22:00 35.0
```

And that's it.

## Compatibility
Should work with all recent windows versions (no, Windows XP is not recent).

## Installing
You need .NET Framework 4.5.2. Then use `installutil.exe` as discribed in [this stackoverflow thread](http://stackoverflow.com/questions/8164859/install-a-windows-service-using-a-windows-command-prompt).
As long as your users do not have admin rights, they won't be able to circumvent this.

## License
3-Clause BSD. Basically: do whatever you want.
