This C# application is a WPF application designed to demonstrate how to use the Microsoft Translator Text Translation APIs. The app gives examples for:

- How to get an access token.
- How to get the list of supported languages for translation from the service.
- How to get the list of supported languages for text-to-speech from the service.
- How to do text-to-text translation.
- How to do text to text-to-speech of a translation.

The Project was created with Visual Studio 2015.

## How to run the app

This sample requires a subscription with Microsoft Translator Text Translation API, which is part of Microsoft Azure Cognitive Services. Visit the [Text Translation API documentation page](http://docs.microsofttranslator.com/text-translate.html) to get started.

Once you have a subscription, do the following to run the app:

- Open the project/solution in Visual Studio.

- In file `MainWindow.xaml.cs`, enter the secret key for your Text Translation API subscription:

  ```
  private const string TEXT_TRANSLATION_API_SUBSCRIPTION_KEY = "ENTER_YOUR_CLIENT_SECRET";
  ```

- Select Start and run the code.
