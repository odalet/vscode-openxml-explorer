module Remoting

open Fable.Core
open Fable.Import.Axios
open Fable.Import.Axios.Globals
open Shared

let inline private toAsync<'a> (resp: JS.Promise<AxiosXHR<'a>>) =
    resp
    |> Promise.map (fun x -> x.data)
    |> Async.AwaitPromise

let private getApiClient (serverHost) : IOpenXmlApi =
    let typeName = nameof(IOpenXmlApi)
    let getRoute methodName =
        serverHost + Route.builder typeName methodName

    {
        getPackageInfo = fun filePath ->
            let data = [filePath]
            axios.post<Document>(getRoute "getPackageInfo", data)
            |> toAsync
            
        getPartContent = fun filePath partUri ->
            let data = [filePath; partUri]
            axios.post<string>(getRoute "getPartContent", data)
            |> toAsync
    }

open Node
open Node.ChildProcess
open Fable.Core.JsInterop    

let private toStr = function
  | U2.Case2(x:Buffer.Buffer) -> 
    x.toString Buffer.BufferEncoding.Utf8
  | U2.Case1(x:string) -> x

let startServer extensionPath =
    let cb (e:ExecError option) stdout' stderr' =
      if e.IsSome then
          printfn $"ExecError: %s{e.Value.ToString()}"
      printfn $"Err: %s{stderr' |> toStr}"
      printfn $"Out: %s{stdout' |> toStr}"

    let opts = createEmpty<ExecOptions>
    opts.cwd <- Some (extensionPath + "/bin")
    childProcess.exec ("dotnet Server.dll", opts, cb) |> ignore

    getApiClient Route.host