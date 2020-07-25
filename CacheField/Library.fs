module CacheField

open System
open System.Collections.Generic

type Factory<'TKey, 'TVal> = ('TKey) -> 'TVal

type CacheField<'TKey, 'TVal> =
   val private factory: Factory<'TKey,'TVal> 
   val mutable private state: 'TVal ValueOption
   val mutable private lastKey: 'TKey
   
   new(f) = {factory = f; state = ValueNone; lastKey = Unchecked.defaultof<'TKey>}
   new(f, key, value) = {factory = f; state = ValueSome value; lastKey = key}
   
   [<CompiledName("ValueByKey")>]
   member this.valueByKey(key: 'TKey) =
        match this.state with
        | ValueSome v -> if EqualityComparer.Default.Equals(key, this.lastKey) then v else this.reloadValue(key)
        | ValueNone -> this.reloadValue(key)
   
    override this.ToString() =
        match this.state with
        | ValueSome v -> v.ToString()
        | ValueNone -> "Not Load"

    override this.Equals(o) =
        match o with
        | :? 'TVal as v -> this.Equals(v)
        | _ -> false
        
    override this.GetHashCode() =
        match this.state with
        | ValueSome v -> EqualityComparer.Default.GetHashCode(v)
        | ValueNone -> -24321
    
    member private this.reloadValue(key) =
        let v = this.factory(key)
        this.lastKey <- key
        this.state <- ValueSome v
        v
        
    interface IEquatable<'TVal> with
        member this.Equals(o) =
            match this.state with
            | ValueSome v -> EqualityComparer.Default.Equals(v, o)
            | ValueNone -> false
                
type ExpireCacheField<'TKey,'TVal> =
    val private factory: Factory<'TKey,'TVal>
    val mutable private state: 'TVal ValueOption
    val private expireTime: TimeSpan
    val mutable private lastCheck: DateTime
    val mutable private lastKey: 'TKey
    
    new(f, expireTime, value, key) = {factory = f; state = ValueSome value; expireTime = expireTime; lastKey = key; lastCheck = DateTime.Now}
    new(f, expireTime) = {factory = f; state = ValueNone; expireTime = expireTime; lastCheck = DateTime.MinValue; lastKey = Unchecked.defaultof<'TKey>}
    
    [<CompiledName("ValueByKey")>]
    member this.valueByKey(key) =
         match this.state with
         | ValueSome v ->
                if (this.isValidKey(key)) && (not <| this.isExpire) then v
                else this.reloadValue(key)
         | ValueNone -> this.reloadValue(key)
             
    override this.ToString() =
        match this.state with
        | ValueSome v -> v.ToString()
        | ValueNone -> "Not Load"
         
    override this.Equals(o) =
        match o with
        | :? 'TVal as v -> this.Equals(v)
        | _ -> false
     
     override this.GetHashCode() =
        match this.state with
        | ValueSome v -> EqualityComparer.Default.GetHashCode(v)
        | ValueNone -> -24321
             
    member private this.isExpire =
        DateTime.Now.Subtract(this.lastCheck) >= this.expireTime
       
    member private this.isValidKey(key) =
        EqualityComparer.Default.Equals(key, this.lastKey)
        
    member private this.reloadValue(key) =
        let v = this.factory(key)
        this.lastKey <- key
        this.state <- ValueSome v
        this.lastCheck <- DateTime.Now
        v
        
    interface IEquatable<'TVal> with
        member this.Equals(o) =
            match this.state with
            | ValueSome v -> EqualityComparer.Default.Equals(v, o)
            | ValueNone -> false
