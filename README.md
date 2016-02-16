
AutoMapper.Collections
================================
Adds ability to map collections to existing collections without re-creating the collection object.

Will Add/Update/Delete items from a preexisting collection object based on user defined equivalency between the collection's generic item type from the source collection and the destination collection.

How to add to AutoMapper?
--------------------------------
Add CollectionProfile to AutoMapper.

	Mapper.AddProfile<CollectionProfile>();
Will add new IObjectMapper objects into the master mapping list.

Adding equivalency between two classes
--------------------------------
Adding equivalence to objects is done with EqualityComparision extended from the IMappingExpression class.

	Mapper.CreateMap<OrderItemDTO, OrderItem>().EqualityComparision((odto, o) => odto.ID == o.ID);
Mapping OrderDTO back to Order will compare Order items list based on if their ID's match

	Mapper.Map<OrderDTO,Order>(orderDto, order);
If ID's match will map OrderDTO to Order

If OrderDTO exists and Order doesn't add to collection

If Order exists and OrderDTO doesn't remove from collection

Why update collection?  Just recreate it 
-------------------------------
ORMs don't like setting the collection, so you need to add and remove from preexisting one.

This automates the process by just specifying what is equal to each other.

Can it just figure out the ID equivalency for me in EF?
-------------------------------
Automapper.Collections.EntityFramework can do that for you.
	
	EquivilentExpressions.GenerateEquality.Add(new GenerateEntityFrameworkPrimaryKeyEquivilentExpressions<TDataContext>());
User defined equality expressions will overwrite primary key expressions.

What about comparing to a single existing Entity for updating?
--------------------------------
Automapper.Collections.EntityFramework does that as well through extension method from of DbSet<TEntity>.

Translate equality between dto and EF object to an expression of just the EF using the dto's values as constants.

	dbContext.Orders.Persist().InsertOrUpdate<OrderDTO>(newOrderDto);
	dbContext.Orders.Persist().InsertOrUpdate<OrderDTO>(existingOrderDto);
	dbContext.Orders.Persist().Remove<OrderDTO>(deletedOrderDto);
	dbContext.SubmitChanges();
Persist doesn't call submit changes automatically

How to get it
--------------------------------
On Nuget

	PM> Install-Package AutoMapper.Collection
	PM> Install-Package AutoMapper.Collection.EntityFramework
