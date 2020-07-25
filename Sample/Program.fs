// Learn more about F# at http://fsharp.org

open System
open System.Threading
open CacheField

module CacheFieldSample =
    let run() =
        let mutable callCount = 0
        let f = CacheField(fun _x ->
            callCount <- callCount + 1
            callCount)
        
        printf "Value: %d\n" (f.valueByKey 1) // create value 1
        printf "Value: %d\n" (f.valueByKey 1) // cache value 1
        printf "Value: %d\n" (f.valueByKey 10) // replace value 2
        printf "Value: %d\n" (f.valueByKey 10) // cache value 2

module ExpireCacheFieldSample =
    let run() =
        let mutable callCount = 0
        let f = ExpireCacheField((fun _x ->
            callCount <- callCount + 1
            callCount), TimeSpan.FromSeconds(float 0.5))
        
        printf "Value: %d\n" (f.valueByKey 1) // create value
        printf "Value: %d\n" (f.valueByKey 1) // cache value

        Thread.Sleep(TimeSpan.FromSeconds(float 1))
        
        printf "Value: %d\n" (f.valueByKey 1) // replace value
        printf "Value: %d\n" (f.valueByKey 1) // cache value
        
        printf "Value: %d\n" (f.valueByKey 10) // replace value
        printf "Value: %d\n" (f.valueByKey 10) // cache value


[<EntryPoint>]
let main argv =
    CacheFieldSample.run()
    
    printf "-------------\n"
    
    ExpireCacheFieldSample.run()
    
    0 // return an integer exit code
