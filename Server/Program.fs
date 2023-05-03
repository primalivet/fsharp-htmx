module Server.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.ViewEngine
open Server.Views.Models
open JsonPlaceholder

// ---------------------------------
// Example
// ---------------------------------

let postHandler (postId: int64) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! post = getPostById postId
            let! comments = getCommentsForPost postId

            return!
                match (post, comments) with
                | (Ok post, Ok comments) ->
                    (Views.Pages.Posts.single (post, comments) |> Views.Layouts.standard |> htmlView) next ctx
                | (Error reason, _) -> (div [] [ encodedText reason ] |> Views.Layouts.standard |> htmlView) next ctx
                | (_, Error reason) -> (div [] [ encodedText reason ] |> Views.Layouts.standard |> htmlView) next ctx
        }

let postsHandler: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let page = ctx.TryGetQueryStringValue "page" |> Option.defaultValue "1" |> int
            let count = ctx.TryGetQueryStringValue "count" |> Option.defaultValue "6" |> int
            let! post = getPostsPerPage (page, count)

            return!
                match post with
                | Ok posts -> (Views.Pages.Posts.listing posts |> Views.Layouts.standard |> htmlView) next ctx
                | Error reason -> (div [] [ encodedText reason ] |> Views.Layouts.standard |> htmlView) next ctx
        }


// ---------------------------------
// Web app
// ---------------------------------

let indexHandler (name: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let greetings = sprintf "Hello %s, from Giraffe!" name
            let model = { Text = greetings }
            return! (Views.Pages.index model |> Views.Layouts.standard |> htmlView) next ctx
        }


let webApp =
    choose
        [ GET
          >=> choose
              [ route "/" >=> indexHandler "world"
                routef "/posts/%d" postHandler
                route "/posts" >=> postsHandler
                route "/sample-payload" >=> text "hello from target route" ]
          setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins("http://localhost:5000", "https://localhost:5001")
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler).UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseContentRoot(contentRoot)
                .UseWebRoot(webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
