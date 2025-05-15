namespace FS_SPA_Practice

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

module Server =


    let People =
        ListModel.FromSeq [
            "John"
            "Paul"
            "George"
            "Ringo"
        ]

    [<Rpc>]
    let GetNames = 
        async {
            return People
        }
        |> Async.StartAsTask

    //[<Rpc>]
    //let Getnames2 =
    //    People
        

    [<Rpc>]
    let AddName (name: string) =
        async {         
            People.Add(name)
        }

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    //let People =
    //    ListModel.FromSeq [
    //        "John"
    //        "Paul"
    //        "George"
    //    ]


    [<SPAEntryPoint>]
    let Main () =
        let newName = Var.Create ""

// Pseudocode:
// 1. LoadNames is a Task<List<string>> (from Server.GetNames).
// 2. To extract the list, use Async.AwaitTask to convert Task to Async, then run the async to get the result.
// 3. In F#, you can use Async.RunSynchronously or bind in an async workflow.
// 4. For UI binding, you typically want to use a View or Var to hold the list and update it when the task completes.

        let namesVar = ListModel.Create<string, string> (fun name -> name) [] // Create an empty ListModel

        let LoadNames =
            async {
                let! names = Server.GetNames |> Async.AwaitTask
                namesVar.Value <- names
            }
            |> Async.Start

        // Now namesVar.View gives you a View<List<string>> you can use in your UI.
        // Example usage in DocSeqCached:

        let AddName name = 
            Server.AddName name

        IndexTemplate.Main()
            .ListContainer(
                namesVar.View.DocSeqCached(fun (name: string) ->            
                    IndexTemplate.ListItem().Name(name).Doc()
                )
            )

            .Name(newName)
            .Add(fun _ ->
                AddName newName.Value
                newName.Value <- ""
            )
            .Doc()
        |> Doc.RunById "main"
