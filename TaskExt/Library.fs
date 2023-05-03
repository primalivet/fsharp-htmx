module Task.Ext
    open System.Threading.Tasks

    let mapT (f: 'a -> 'b) (t: Task<'a>) : Task<'b> =
        task {
            let! value = t
            return value |> f
        }

    let bindT (f: 'a -> Task<'b>) (t: Task<'a>) : Task<'b> =
        task {
            let! value = t
            return! f value
        }
