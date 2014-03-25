del ..\*.nupkg
cd Stencil.Base
nuget pack -Prop Configuration=Release
move *.nupkg ..\..
cd ..\Stencil.Render\Bitmap
nuget pack -Prop Configuration=Release
move *.nupkg ..\..\..
cd ..\..