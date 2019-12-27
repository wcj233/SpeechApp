using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpeechApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string ON = "enabled";
        private const string OFF = "disabled";
        private SpeechRecognizer speechRecognizer;
        private CoreDispatcher dispatcher;
        private StringBuilder dictateBuilder;
        private bool isListening;

        public MainPage()
        {
            this.InitializeComponent();

            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            dictateBuilder = new StringBuilder();
            isListening = false;
        }

        private void Cotent_TextChanged(object sender, RoutedEventArgs e)
        {

            richEbitBox.Document.GetText(TextGetOptions.None, out string value);
        }

        private async void Actions_Click(object sender, RoutedEventArgs e)
        {

            richEbitBox.Document.Selection.SetRange(0, richEbitBox.Document.Selection.EndPosition);

            var id = sender as Button;

            richEbitBox.Focus(FocusState.Pointer);

            switch (id.Tag)
            {
                case "0":
                    if (isListening == false)
                    {
                        isListening = true;
                        speechRecognizer = new SpeechRecognizer();
                        var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                        speechRecognizer.Constraints.Add(dictationConstraint);
                        SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
                        speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_CompletedAsync;
                        speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                        speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
                        await speechRecognizer.ContinuousRecognitionSession.StartAsync();

                    }
                    else
                    {
                        isListening = false;
                        if (speechRecognizer.State != SpeechRecognizerState.Idle)
                        {
                            await speechRecognizer.ContinuousRecognitionSession.StopAsync();
                            speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_CompletedAsync;
                            speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                            speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;
                            speechRecognizer.Dispose();
                            speechRecognizer = null;
                        }

                    }
                    break;
                case "1":
                    if (richEbitBox.Document.Selection.CharacterFormat.Bold == FormatEffect.On)
                    {
                        richEbitBox.Document.Selection.CharacterFormat.Bold = FormatEffect.Off;
                    }
                    else
                    {
                        richEbitBox.Document.Selection.CharacterFormat.Bold = FormatEffect.On;
                    }
                    break;
                case "2":
                    if (richEbitBox.Document.Selection.CharacterFormat.Italic == FormatEffect.On)
                    {
                        richEbitBox.Document.Selection.CharacterFormat.Italic = FormatEffect.Off;
                    }
                    else
                    {
                        richEbitBox.Document.Selection.CharacterFormat.Italic = FormatEffect.On;
                    }
                    break;
                case "3":
                    if (richEbitBox.Document.Selection.CharacterFormat.Underline == UnderlineType.Single)
                    {
                        richEbitBox.Document.Selection.CharacterFormat.Underline = UnderlineType.None;
                    }
                    else
                    {
                        richEbitBox.Document.Selection.CharacterFormat.Underline = UnderlineType.Single;
                    }
                    break;
                case "5":
                    richEbitBox.Document.GetText(TextGetOptions.AdjustCrlf, out string value);
                    speak(value);
                    break;
                default:
                    break;
            }
        }

        private async void SpeechRecognizer_HypothesisGenerated(
            SpeechRecognizer sender,
            SpeechRecognitionHypothesisGeneratedEventArgs args)
        {

            string hypothesis = args.Hypothesis.Text;
            string textboxContent = dictateBuilder.ToString() + " " + hypothesis + " ...";

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                richEbitBox.Document.SetText(TextSetOptions.None, textboxContent);
            });
        }

        private async void ContinuousRecognitionSession_ResultGenerated(
            SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {

            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                  args.Result.Confidence == SpeechRecognitionConfidence.High)
            {

                dictateBuilder.Append(args.Result.Text + " ");

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    richEbitBox.Document.SetText(TextSetOptions.None, dictateBuilder.ToString());
                });
            }
        }

        private async void ContinuousRecognitionSession_CompletedAsync(
                SpeechContinuousRecognitionSession sender,
                SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                isListening = false;
            }

        }
        private async void speak(string value)
        {

            MediaElement mediaElement = new MediaElement();

            var synth = new SpeechSynthesizer();

            VoiceInformation voiceInfo = (from voice in SpeechSynthesizer.AllVoices
                                          where voice.Gender == VoiceGender.Female
                                          select voice).FirstOrDefault();

            synth.Voice = voiceInfo;

            // Generate the audio stream from plain text.
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(value);

            // Send the stream to the media object.
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private void ComboChanged(object sender, SelectionChangedEventArgs e)
        {

            richEbitBox.Focus(FocusState.Pointer);

            var id = sender as ComboBox;
            switch (id.Tag)
            {

                case "1":
                    string fontName = id.SelectedItem.ToString();
                    richEbitBox.Document.Selection.CharacterFormat.Name = fontName;
                    break;
                case "2":
                    var size = id.SelectedItem.ToString();
                    //set size to the Selection
                    richEbitBox.Document.Selection.CharacterFormat.Size = Convert.ToInt32(size);
                    break;
                default:
                    break;
            }
        }

        private void fontBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void fontBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            richEbitBox.Focus(FocusState.Pointer);
        }
    }
}

