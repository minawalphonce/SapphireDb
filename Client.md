# ng-realtime-database

Realtime Database Client - Angular Configuration

## Installation

### Install Package
To use realtime database in client you have to install the package using node.js

In your Angular App-Folder execute

```
npm install ng-realtime-database linq4js -S
```

### Import realtime database module in your app.module

```
imports: [
    BrowserModule,
    ...,
    RealtimeDatabaseModule, 
]
```

or using custom configuration

```
imports: [
    BrowserModule,
    ...,
    RealtimeDatabaseModule.config({
        serverBaseUrl: `${location.hostname}:${location.port}`,
        useSecuredSocket: false
    }) 
]
```

### Use it where you need it

To access the realtime database you need to inject realtime database where you need it.

Example:
```
@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.less']
})
export class MainComponent implements OnInit {

  constructor(private db: RealtimeDatabase) { }

  ngOnInit() {
    this.db.collection('example').values().subscribe(console.log);
  }
}
```

## Query data

To query data you can just execute `.snapshot()` on your collection.

```js
// returns Observable<IUser[]>
const collection = this.db.collection<IUser>('users').snapshot();
```

The `.snapshot()` function only queries a snapshot of your collection.
The client will not get updates, after the query was made.
The observable completes after the client received the data.

## Subscribe data

If you want your client to also get notified when the collection changes
you have to use `.values()`.

```js
// returns Observable<IUser[]>
const collection = this.db.collection<IUser>('users').values();
```

The `.values()` function returns an observable
with the values of the collection. If the collection changes the
observable emits the new array. The observable does not complete,
so make sure to unsubscribe it.

## Filter data

If you dont want to query the whole collection in your client you
can use prefilters in both methods `.snapshot()` and `.values()`.
The prefilter filters the data at server side and only sends relevant data
to the client.

Example: You only want the last 10 values of your collection,
ordered by username.

```js
const collection = this.db.collection<IUser>('user').snapshot(
  new OrderByPrefilter('id', true),
  new TakePrefilter(10),
  new OrderByPrefilter('username')
);

const collection2 = this.db.collection<IUser>('user').values(
  new OrderByPrefilter('id', true),
  new TakePrefilter(10),
  new OrderByPrefilter('username')
);
```

### Available filter

#### WherePrefilter

This prefilter is used to filter your data.

The syntax is:
```js
// propertyName: needs to be a valid property in your model
// comparison: '==' | '!=' | '<' | '<='| '>' | '>='
// compareValue: any value you want compare with the property
new WherePrefilter(propertyName, comparison, compareValue);
```

Example: Only take vales with username test
```js
new WherePrefilter('username', '==', 'test');
```

#### OrderByPrefilter

This prefilter is used for sorting.

The syntax is:
```js
// propertyName: needs to be a valid property in your model
// descending: false | true
new OrderByPrefilter(propertyName, descending);
```

Example: Order the collection by username from z-a (descending)
```js
new OrderByPrefilter('username', true);
```

#### ThenOrderByPrefilter

This prefilter is used for sorting by a second or more properties.
You first have to first use an OrderByPrefilter.

The syntax is:
```js
// propertyName: needs to be a valid property in your model
// descending: false | true
new ThenOrderByPrefilter(propertyName, descending);
```

Example: Order the collection by username from z-a (descending)
and then by name.
```js
new OrderByPrefilter('username', true),
new ThenOrderByPrefilter('name')
```

#### SkipPrefilter

This prefilter is used to skip a specific count of items in your collection.

The syntax is:
```js
// number: The number of items to skip
new SkipPrefilter(number);
```

Example: Skip the first 15 values
```js
new SkipPrefilter(15);
```

#### TakePrefilter

This prefilter is used to limit the number of items queried.

The syntax is:
```js
// number: The number of items to take
new TakePrefilter(number);
```

Example: Only take a maximum of 15 items
```js
new TakePrefilter(15);
```

## Add data

To add data use `.add()` function on collection

Example: Add User to collection user

```js
this.db.collection('user').add({
  username: 'test123',
  name: 'Test User'
});
```

The value to add has to be a valid model. You can use annotations at server side
to check the model. If there are validation errors the returned observable of the
function will emit an object with information about it.

## Update data

To update data use `.update()` function on collection

Example: Add User in collection user

```js
this.db.collection('user').update({
  id: 32,
  username: 'test123',
  name: 'Test User'
});
```

The new model has to be valid. Also you need to send the primary keys of the
model.

## Remove data

To delete data use `.remove()` function on collection

Example: Remove User from collection user

```js
this.db.collection('user').remove({
  id: 32,
  username: 'test123',
  name: 'Test User'
});
```

The model is removed from the collection using the primary key(s). You only need
to pass this information to the server.

## Security

If you want to use a JWT to secure your backend you have to
also send it to realtime database. This can be done by using the function
`.setBearer()`

Example:
```js
db.setBearer('example JWT Token');
```

The websocket will now refresh and use the JWT.

Make sure to configure the backend to load the JWT as query param.
Checkout [Server Configuration](Server.md) to learn more.

### Check if role(s) has access

If you use the `RealtimeAuthorize` Attribute on your models you can get information
about the access using the methods `canRead()`, `canWrite()` and `canDelete()`. You can also
check if the model needs authentication by using `onlyAuthenticated()`.

Example: Check if role admin can delete data in collection user

```js
this.db.collection('user').canDelete(['admin'])
```