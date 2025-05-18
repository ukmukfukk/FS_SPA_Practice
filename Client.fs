namespace FS_SPA_Practice

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating

module Server =
    let mutable PeopleList = [
            "John"
            "Paul"
            "George"
            "Ringo"
        ]

    [<Rpc>]
    let AddName (name: string) =
        async {         
            PeopleList <- PeopleList @ [name]
            printfn "%A" name
            printfn "%A" PeopleList
        }

    [<Rpc>]        
    let GetUsers () =
        async {
            printfn "GetUsers %A" PeopleList
            return PeopleList
        }

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        let newName = Var.Create "" // Create a Var to hold the new name

        let People = ListModel.Create<string, string> (fun name -> name) [] // Create an empty ListModel 

        let LoadUsersAsync =
            async {
                let! users = Server.GetUsers()
                People.Set(users) // Set the ListModel with the names from the server)        
        }           

        // This is not working for fethcing, because it's not an async, just a Val
        let LoadUsers =
            LoadUsersAsync
            |> Async.StartImmediate            

        let AddName name =
            Server.AddName name |> Async.Start

        IndexTemplate.Main()
            .ListContainer(
                People.View.DocSeqCached(fun (name: string) ->            
                    IndexTemplate.ListItem().Name(name).Doc()
                )
            )
            .LoadUsers(fun e -> 
                LoadUsersAsync 
                |> Async.StartImmediate
            )
            .Name(newName)
            .Add(fun _ -> 
                AddName newName.Value
                newName.Set ""
                LoadUsersAsync
                |> Async.StartImmediate
                )
            .Doc()
        |> Doc.RunById "main"
