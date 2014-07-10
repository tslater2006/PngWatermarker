#PngWatermarker 


PngWatermarker is a .NET 4.5+ library for the embedding and extraction of invisible watermarks on PNG files.

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
