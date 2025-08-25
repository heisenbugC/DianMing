For introduction and usage of this app, please refer to [my blog post](https://heisenbugta.blogspot.com/2025/08/40.html).
 
To enable post-Windows Xp visual styles, publish the application using ClickOnce First, then add
the themes.manifest by copying it to the /publish/app_name_version_number/ ,
then open developer shell in this directory and run the following command:
```
mt.exe -manifest themes.manifest -outputresource:点名.exe.deploy
ren 点名.exe.deploy 点名.exe
```
