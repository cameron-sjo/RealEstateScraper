# How To Search

The search functionality was loosely based off of [StackExchange's search functionality][stackexchange_search].

It requires keywords and parameters to function.

Examples:
 - `description:dream` - includes all listings with a description that contains the word `dream`
 - `price:>75000` - includes all listings with a price over 75,000

It also allows for ranges on numeric types.

Examples:
 - `price:75000...500000` - includes all listings between 75000 and 500000

Last but not least, you can have multiple qualifiers!

Examples:
 - `price:<250000 bathrooms:>=2 bedrooms:>=3` 
 - `price:0...100000 kind:land`

[stackexchange_search]:https://stackoverflow.com/help/searching