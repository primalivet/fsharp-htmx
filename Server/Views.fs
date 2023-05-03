module Server.Views

module Models =
    type Message = { Text: string }

    type Listable =
        { Id: int64
          Href: string
          Title: string
          Body: string }

module Components =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Htmx
    open JsonPlaceholder

    let blueLink href text =
        a [ _href href; _class "underline text-blue-600" ] [ encodedText text ]

    let primaryButton attrs text =
        button (List.append [ _type "button"; _class "text-slate-200 bg-slate-900" ] attrs) [ encodedText text ]

    module Posts =
        let compactPost (post: Post) =
            li
                [ _hxBoost; _id (string post.id) ]
                [ h3 [ _class "text-lg-blue-600 mb-2" ] [ blueLink (sprintf "/posts/%d" post.id) post.title ]
                  p [] [ encodedText post.body ] ]


module Layouts =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Htmx

    let standard (content: XmlNode) =
        let menu () =
            let links = [ ("/", "Home"); ("/posts", "Posts") ]

            nav
                [ _class "p-4 bg-slate-200" ]
                [ ul
                      [ _class "flex gap-2" ]
                      (links
                       |> List.map (fun (href, text) ->
                           li
                               [ _hxBoost ]
                               [ a [ _class "py-1 px-2 rounded bg-slate-300"; _href href ] [ encodedText text ] ])) ]

        html
            []
            [ head
                  []
                  [ title [] [ encodedText "Server" ]
                    link [ _rel "stylesheet"; _type "text/css"; _href "/main.css" ]
                    script [ _src "https://cdn.tailwindcss.com" ] []
                    Htmx.Script.unminified ]
              body [] [ main [ _id "main" ] [ menu (); content ] ] ]

    let none (content: XmlNode) = content

module Pages =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Htmx

    module Posts =
        open JsonPlaceholder

        let listing (posts: Post list) =
            div
                [ _class "p-4" ]
                [ ul [ _class "flex flex-col gap-4" ] (posts |> List.map (Components.Posts.compactPost)) ]

        let single (post: Post) =
            div
                [ _class "p-4" ]
                [ nav [ _hxBoost ] [ Components.blueLink "/posts" "<- All items" ]
                  h2 [] [ encodedText post.title ]
                  p [] [ encodedText post.body ] ]

    let index (model: Models.Message) =
        div
            []
            [ p [] [ encodedText model.Text ]
              Components.primaryButton
                  [ _hxTrigger HxTrigger.Click
                    _hxGet "/sample-payload"
                    _hxTarget "#target-div" ]
                  "hello button"
              div [ _id "target-div" ] [ encodedText "target div" ] ]
