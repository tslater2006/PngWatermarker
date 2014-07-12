#PngWatermarker 

PngWatermarker is a .NET 4.5+ library for the embedding and extraction of invisible watermarks on PNG files.

It makes use of the lovely pngcs library for decoding/encoding of the PNG files, the library was written by Hernán J. González, which is available here: https://code.google.com/p/pngcs/

##Features

  - Embed various watermarks into PNG files
  - Extract the watermarks at a later time
  - A watermarked image is almost impossible to distinguish.
  - Clever embedding process ensures the watermark is not stored in a sequential pattern

##Watermark Types

###Basic
* Text Watermark
 * Allows for the storage of a String.
* File Watermark
 * Allows for the storage of a file, preserving the original extension.
* Binary Watermark
 * Allows for the storage of binary data. 

###Advanced
* Composite Watermark
 * Allows for the storage of 1 to many basic watermarks as a single watermark in the image.
* Encrypted Watermark
 * Allows for the encryption and storage of any watermark (except other Encrypted Watermarks).
 * Encryption is performed by any SymmetricAlgorithm subclass.

##Examples

###Text Watermark

```C#
PNGFile file = new PNGFile("MyOriginal.png");
TextWatermark mark = new TextWatermark("This is a text based watermark");

Watermarker.EmbedWatermark(file,mark,"password","MyOutput.png");

//Extraction

file = new PNGFile("MyOutput.png");
TextWatermark extract = (TextWatermark)Watermarker.ExtractWatermark(file,"password");

```

###Composite Watermark

```C#
PNGFile file = new PNGFile("MyOriginal.png");

CompositeWatermark comp = new CompositeWatermark();
FileWatermark fileMark = new FileWatermark("MyFile.txt");
BinaryWatermark binMark = new BinaryWatermark(new byte[]{1,2,3,4});

comp.AddWatermark(fileMark);
comp.AddWatermark(binMark);

Watermarker.EmbedWatermark(file,comp,"password","MyOutput.png");

//Extraction
PNGFile file2 = new PNGFile("MyOutput.png");

CompositeWatermark extract = (CompositeWatermark)Watermarker.ExtractWatermark(file2,"password");
Watermark[] marks = extract.GetWatermarks();

```

###Encrypted Watermark
```C#
PNGFile file = new PNGFile("MyOriginal.png");
RijndaelManaged aes = new RijndaelManaged();
aes.Padding = PaddingMode.Zeroes;
EncryptedWatermark.Algorithm = aes;
TextWatermark mark = new TextWatermark("This should be encrypted");
EncryptedWatermark encrypted = new EncryptedWatermark(mark, "super-secret");

Watermarker.EmbedWatermark(file, encrypted, "password", "MyOutput.png");

PNGFile file2 = new PNGFile("MyOutput.png");

EncryptedWatermark extract = Watermarker.ExtractWatermark(file2, "password");
extract.Decrypt("super-secret");

Watermark decrypted = extract.DecryptedMark;
```

## Original File
![Original File](https://tslater2006.github.io/PngWatermarker/images/Flower_Original.png)

## File holding a watermark
![Watermarked File](https://tslater2006.github.io/PngWatermarker/images/TextMark.png)


[![githalytics.com alpha](https://cruel-carlota.pagodabox.com/588832789e96c4c386c38c99c9cd8bb1 "githalytics.com")](http://githalytics.com/tslater2006/PngWatermarker)
