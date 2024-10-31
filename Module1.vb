Imports LLama.Common
Imports LLama
Imports System.IO
Imports LLama.Sampling

Module Module1

    Sub Main()

        Console.Title = "SIWSIWIT"

        ' can be downloaded in https://huggingface.co/TheBloke/phi-2-GGUF
        Dim modelPath = Path.Combine(Directory.GetCurrentDirectory(), "phi-2.Q8_0.gguf")

        Dim parameters = New ModelParams(modelPath) With {
            .ContextSize = 1024,
            .GpuLayerCount = 10
        }

        Using model = LLamaWeights.LoadFromFile(parameters)
            Using context = model.CreateContext(parameters)
                Dim executor = New InteractiveExecutor(context)

                Dim chatHistory = New ChatHistory()
                chatHistory.AddMessage(AuthorRole.System, "Transcript of a dialog, where the User interacts with an Assistant named Birdie. Birdie is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision. Dont include 'User:' or 'Assistant:' on your response.")
                chatHistory.AddMessage(AuthorRole.User, "Hello, Birdie.")
                chatHistory.AddMessage(AuthorRole.Assistant, "Hello. How may I help you today?")

                Dim session = New ChatSession(executor, chatHistory)
                session.WithOutputTransform(New LLamaTransforms.KeywordTextOutputStreamTransform({"User:", "�", "Assistant:"}, 5))

                Dim inferenceParams = New InferenceParams() With {
                    .AntiPrompts = {"User:"}.ToList,
                    .MaxTokens = 256,
                    .SamplingPipeline = New DefaultSamplingPipeline() With {
                        .Temperature = 0.9
                    }
                }

                Console.Clear()
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.WriteLine("> Hello World!")
                Console.WriteLine("> How may I help you today?")
                Console.ForegroundColor = ConsoleColor.White

                Do While True
                    Dim userInput = Console.ReadLine()
                    If String.IsNullOrWhiteSpace(userInput) = False Then
                        If userInput.ToLower = "stop" Then Exit Do

                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.WriteLine("generating response ... please wait ... ")

                        Dim response As IAsyncEnumerable(Of String) = session.ChatAsync(New ChatHistory.Message(AuthorRole.User, userInput), inferenceParams)
                        Dim str1 As New List(Of String)
                        For Each r In response.ToListAsync().Result
                            str1.Add(r)
                        Next

                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.WriteLine(String.Join("", str1).Trim)
                        Console.ForegroundColor = ConsoleColor.White
                    End If
                Loop

            End Using
        End Using

    End Sub

End Module
