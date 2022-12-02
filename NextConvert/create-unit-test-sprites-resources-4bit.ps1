NextConvert\bin\Debug\net7.0\NextConvert.exe --transparent "#FF00FF" --sheet-background "#011E2B" --sheet-image-columns 5 --sheet-palette-columns 8 --keep-transparent boxed sprites --4bit --in-sprites UnitTests\Resources\Project-Sprites-Image.bmp --out-sheet UnitTests\Resources\export-sprites-sheet-4bit.bmp

Remove-Item .\UnitTests\Resources\export-sprites-image*.bmp

Get-ChildItem -Path ".\UnitTests\Resources\export-sprites-sheet-4bit[0-9]*.bmp" | Rename-Item -NewName { $_.name -replace "sheet-4bit","image" }