using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ArgsParserNS;

namespace TerrariaPlayerDataEncryptDecrypt
{
    public class Program
    {
		// The program name displayed in help
		public const string PROGRAM_NAME = "TerrariaPlayerDataEncryptDecrypt";
		// The program version displayed in help
		public const string PROGRAM_VERSION = "1.0";
		// The program description displayed in help
		public const string PROGRAM_DESCRIPTION = "an encryptor/decryptor for Terraria's player data files";
		// The name used for displaying logs
		public static readonly string ASSEMBLY_NAME = Assembly.GetExecutingAssembly().GetName().Name;
		// The encryption key used for player data files, this was grabbed from Terraria 1.4.4.9
		public static readonly byte[] ENCRYPTION_KEY = Encoding.Unicode.GetBytes("h3y_gUyZ");

		static void Main(string[] args)
        {
			// Create the GNU style arguments parser
			ArgsParser argsParser = new ArgsParser();

			// Register the valid operations as arguments
			argsParser.RegisteredArguments.Add(new Argument()
			{
				Name = "encrypt",
				ShortName = "e"
			});
			argsParser.RegisteredArguments.Add(new Argument()
			{
				Name = "decrypt",
				ShortName = "d"
			});
			argsParser.RegisteredArguments.Add(new Argument()
			{
				Name = "help",
				ShortName = "h"
			});

			// Parse the passed-in arguments
			FilledArgument[] parsedArgs = argsParser.ParseArguments(args);
			// Create variables that will hold our operations
			FilledArgument encrypt = null;
			FilledArgument decrypt = null;
			FilledArgument help = null;

			// Try to get the "encrypt" operation
			try
			{
				encrypt = parsedArgs.First((FilledArgument arg)
					=> { return arg.Arg.Name.Contains("encrypt"); });
			}
			catch { }

			// Try to get the "decrypt" operation
			try
			{
				decrypt = parsedArgs.First((FilledArgument arg)
					=> { return arg.Arg.Name.Contains("decrypt"); });
			}
			catch { }

			// Try to get the "Help" operation
			try
			{
				help = parsedArgs.First((FilledArgument arg)
					=> { return arg.Arg.Name.Contains("help"); });
			}
			catch { }

			// Check if no valid operation was passed
			// Check if both encrypt and decrypt operations were passed
			// Check encrypt was passed, but no path provided
			// Check decrypt was passed, but no path provided
			// If any of the conditions mentioned above are met, display an error about the wrong usage
			if ((help == null && encrypt == null && decrypt == null) || 
				(encrypt != null && decrypt != null) || 
				(encrypt != null && encrypt.Value == null) || 
				(decrypt != null && decrypt.Value == null)) 
			{
				Console.Error.WriteLine($"{ASSEMBLY_NAME}: wrong usage");
				DisplayHelp(true);
				Console.WriteLine();
				Console.WriteLine($"Try '{ASSEMBLY_NAME} --help' for more options.");
				return;
			}

			// The operation the program will perform
			string mode = encrypt != null ? 
				"e" : decrypt != null ? 
				"d" : help != null ? "h" : throw new Exception();

			byte[] fileData = null;
			byte[] finalFileData = null;
			string inputPath = mode == "e" ? encrypt.Value : mode == "d" ? decrypt.Value : "";
			string inputFileName = null;
			string outputPath = null;
			string outputFileName = null;

			if (mode != "h") 
			{
				if (!File.Exists(inputPath))
				{
					Console.Error.WriteLine($"{ASSEMBLY_NAME}: file not found");
					return;
				}

				inputFileName = Path.GetFileName(inputPath);
				// Create the path for the output file based on the operation
				outputPath = Path.Combine(Path.GetDirectoryName(inputPath),
					$"{(mode == "e" ? "encrypted_" : mode == "d" ? "decrypted_" : "")}" +
					$"{Path.GetFileName(inputPath)}");
				outputFileName = Path.GetFileName(outputPath);

				try
				{
					fileData = File.ReadAllBytes(inputPath);
				}
				catch (Exception ex)
				{
					DisplayException($"unable to read file \"{inputPath}\"", ex);
					return;
				}
			}

			// Execute the specified operation
			switch (mode) 
			{
				case "e":
					Console.WriteLine($"{ASSEMBLY_NAME}: encrypting {inputFileName} as {outputFileName}...");

                    try 
					{
						finalFileData = TransformPlayerData(fileData, false);
					}
					catch (Exception ex) 
					{
						DisplayException($"unable to encrypt file \"{inputPath}\"", ex);
						return;
					}

					try
					{
						File.WriteAllBytes(outputPath, finalFileData);
					}
					catch (Exception ex)
					{
						DisplayException($"unable to write to file \"{outputPath}\"", ex);
						return;
					}
					
					Console.WriteLine($"{ASSEMBLY_NAME}: encrypted {inputFileName} as {outputPath}");

					break;
                case "d":
					Console.WriteLine($"{ASSEMBLY_NAME}: decrypting {inputFileName} as {outputFileName}...");

					try
					{
						finalFileData = TransformPlayerData(fileData, true);
					}
					catch (Exception ex)
					{
						DisplayException($"unable to decrypt file \"{inputPath}\"", ex);
						return;
					}

					try
					{
						File.WriteAllBytes(outputPath, finalFileData);
					}
					catch (Exception ex)
					{
						DisplayException($"unable to write to file \"{outputPath}\"", ex);
						return;
					}

					Console.WriteLine($"{ASSEMBLY_NAME}: decrypted {inputFileName} as {outputPath}");

					break;
				case "h":
					DisplayHelp(false);
					break;
			}
        }
		
        public static byte[] TransformPlayerData(byte[] playerData, bool decrypt) 
		{
			byte[] transformedPlayerData;
			RijndaelManaged rijndaelManaged = new RijndaelManaged();
			// Terraria also disables padding
			rijndaelManaged.Padding = PaddingMode.None;

			// Create a buffer to hold our transformed data
			MemoryStream memoryStream = new MemoryStream();
			// Create a stream that will transform the input data into the buffer
			CryptoStream cryptoStream = new CryptoStream(memoryStream,
				decrypt ? rijndaelManaged.CreateDecryptor(ENCRYPTION_KEY, ENCRYPTION_KEY) : 
				rijndaelManaged.CreateEncryptor(ENCRYPTION_KEY, ENCRYPTION_KEY),
				CryptoStreamMode.Write);
			// Transform  the data
			cryptoStream.Write(playerData, 0, playerData.Length);
			cryptoStream.FlushFinalBlock();
			transformedPlayerData = memoryStream.ToArray();

			// Clean-up
			cryptoStream.Close();
			memoryStream.Close();
			cryptoStream.Dispose();
			memoryStream.Dispose();

			return transformedPlayerData;
		}

		public static void DisplayException(string msg, Exception ex) 
		{
			Console.Error.WriteLine($"{ASSEMBLY_NAME}: {msg}");
			Console.Error.WriteLine($"{ASSEMBLY_NAME}: {ex}");
		}
		
		public static void DisplayHelp(bool onlyUsage) 
		{
			if (!onlyUsage) Console.WriteLine($"{PROGRAM_NAME} {PROGRAM_VERSION}, {PROGRAM_DESCRIPTION}");
			Console.WriteLine($"Usage: {ASSEMBLY_NAME} <OPERATION>");
			if (onlyUsage) return;
			Console.WriteLine();
			Console.WriteLine("Mandatory arguments to long operations are mandatory for short operations too.");
			Console.WriteLine();
			Console.WriteLine("Operations:");
			Console.WriteLine("  -e,  --encrypt=PATH  encrypt the specified file.");
			Console.WriteLine("  -d,  --decrypt=PATH  decrypt the specified file.");
			Console.WriteLine("  -h,  --help          print this help.");
		}
	}
}
