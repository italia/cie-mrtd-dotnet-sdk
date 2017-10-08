# CIE-MRTD-SDK FORK

Sdk for reading the ICAO MRTD information from the Italian Electronic Identity Card (CIE).

## Getting Started
Starting from the basic implementation provided from the TeamPerLaTrasformazioneDigitale and available on the git hub platform , the RF-Team (Giovanni Salzillo & Marco Costanzo) developed an easy implementation of this technology in order to facilitate other developers to understand how to use this SDK.
We developed a full working desktop app (based on C# & .Net) that reads card DGs and displays user's data and photo, exports and imports data in different machine formats (CSV, XML & JSON), and finally providing a thin layer of authentication & authorization (a simple check on the existence of already known export files & MRZ matching).
We managed to extract all the kind of informations available on the CIE testing card, processing all the ICAO MRTD informations using a tailor-made & lightweight parser.
We had many problems at manipulating the jpg2000 images. Finally we managed to get our module to work, converting images to bitmaps and displaying them on our application. The images are exported as well.
We centered ourself around the User Object Oriented Model, writing down a class that provides methods and attributes to acquire, process & manipulate, show and export CIE user informations.
Finally, we implemented a very basic and rough GUI.

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.


### Prerequisites

What you need to install.

```
Visual Studio 2017 Community Edition - .Net Framework 4.5
CSJ2K - Portable JPEG 2000 codec library - This library can be easily downloaded from the Visual Studio NuGet Packet Manager.
```


## Project Class tree
The project mantains the old project structure adding the following classes:
```
- ParseLib			: Our Namespace.
	- C_CIE			: Our User Model (Logical Data Struct) + ICAO MRTD information Parsing + Data Import/Export.
	- ArrayUtils	: Array Manipulation.
```
The Program.cs file (In the "Test" namespace) is the application starting point. It has been heavily modified in order to add new functionalities.
	
We performed many unit test, testing our app in all it's main use cases. No particular problem has been detected.

### And coding style tests

Every Class method & attribute is documented inside it's source files. Below a list of the main class methods developed and used in this project.
```
- ParseLib
	-C_CIE
		- Byte[] Image_retrive(Byte [] Blob); 					It extracts a jpeg2000 image from an array bytes based on it's magic number.
		- String ICAOGetValueFromKey(byte [] key, byte [] dg);	It extracts data from tags.
		- C_CIE(EAC eac);										Class Constructor - Fill data attributes queryng an EAC Object.
		- Bitmap ret_cie_bitmap();								Return the user bitmap image.
		- saveOnXML(string path), saveOnCSV(string path), 
			saveOnJSON(string path), readFromXML(string path),  Self Explaining methods.
			readFromCSV(string path), readFromJSONSelf(string path)

```

## Acknowledgments
We would like to thank the whole Stack Overflow Community.
Usefull link - https://stackoverflow.com/questions/943635/getting-a-sub-array-from-an-existing-array

