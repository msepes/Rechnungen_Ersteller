namespace ToolBox
open  FParsec
 open System
 open System.Runtime.InteropServices

module TemplateParser =
    exception ParseError of string

    type Token = KeyWord  of string * string
                | Literal of string

    type Token with
     member x.TryGetKeyWord([<Out>] keyWord:byref<(string * string)>) =
       match x with
       | KeyWord (object,property)-> keyWord <- (object,property); true
       | _ -> false
     member x.TryLiteral([<Out>] literal:byref<string>) =
       match x with
       | Literal value -> literal <- value; true
       | _ -> false

    let Parse template =
        
        let Point = pchar '.'
        let OpenBracket = pchar '{'
        let CloseBracket = pchar '}'
        let Alphabet =  anyOf ['A'..'z']

        let IgnoreBracket = (lookAhead OpenBracket) |>> fun(c) -> ()
        let literal = (many1CharsTill anyChar (IgnoreBracket <|> eof)) |>> fun(str) -> Literal str

        let KeyWord = (OpenBracket >>. (many1CharsTill Alphabet Point .>>. many1CharsTill Alphabet CloseBracket))  |>> fun(obj,prop) -> KeyWord (obj, prop)

        let Token = KeyWord <|> literal

        let templ = many  Token 
        let result = run templ template
        match result with
           | Success(result, _, _)   ->  result
           | Failure(errorMsg, _, _) -> raise (ParseError(errorMsg))
