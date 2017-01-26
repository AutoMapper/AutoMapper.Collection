
AutoMapper.Collection
================================
Adds ability to map collections to existing collections without re-creating the collection object.

Will Add/Update/Delete items from a preexisting collection object based on user defined equivalency between the collection's generic item type from the source collection and the destination collection.

How to add to AutoMapper?
--------------------------------
Call AddCollectionMappers when configuring

	Mapper.Initialize(cfg =>
            {
                cfg.AddCollectionMappers();
				// Configuration code
            });
Will add new IObjectMapper objects into the master mapping list.

Adding equivalency between two classes
--------------------------------
Adding equivalence to objects is done with EqualityComparison extended from the IMappingExpression class.

	cfg.CreateMap<OrderItemDTO, OrderItem>().EqualityComparison((odto, o) => odto.ID == o.ID);
Mapping OrderDTO back to Order will compare Order items list based on if their ID's match

	Mapper.Map<OrderDTO[],Order[]>(orderDtos, orders);
If ID's match will map OrderDTO to Order

If OrderDTO exists and Order doesn't add to collection

If Order exists and OrderDTO doesn't remove from collection

Why update collection?  Just recreate it 
-------------------------------
ORMs don't like setting the collection, so you need to add and remove from preexisting one.

This automates the process by just specifying what is equal to each other.

Can it just figure out the ID equivalency for me in EF?
-------------------------------
Automapper.Collection.EntityFramework can do that for you.
	
	Mapper.Initialize(cfg =>
            {
                cfg.AddCollectionMappers();
                cfg.SetGeneratePropertyMaps<GenerateEntityFrameworkPrimaryKeyPropertyMaps<DB>>();
				// Configuration code
            });
User defined equality expressions will overwrite primary key expressions.

What about comparing to a single existing Entity for updating?
--------------------------------
Automapper.Collection.EntityFramework does that as well through extension method from of DbSet<TEntity>.

Translate equality between dto and EF object to an expression of just the EF using the dto's values as constants.

	dbContext.Orders.Persist().InsertOrUpdate<OrderDTO>(newOrderDto);
	dbContext.Orders.Persist().InsertOrUpdate<OrderDTO>(existingOrderDto);
	dbContext.Orders.Persist().Remove<OrderDTO>(deletedOrderDto);
	dbContext.SubmitChanges();
**Note:** This is done by converting the OrderDTO to Expression<Func<Order,bool>> and using that to find matching type in the database.  You can also map objects to expressions as well.

Persist doesn't call submit changes automatically

How to get it
--------------------------------
On Nuget

	PM> Install-Package AutoMapper.Collection
	PM> Install-Package AutoMapper.Collection.EntityFramework
Also have AutoMapper.LinqToSQL

	PM> Install-Package AutoMapper.Collection.LinqToSQL