﻿namespace SfaChatGraph.Server.Utils
{
	public static class Queries
	{
		public static string ListGraphsQuery() => LIST_GRAPHS;
		public const string LIST_GRAPHS = "SELECT DISTINCT ?g WHERE { GRAPH ?g { } }";

		public static string SchemaQuery(int offset = 0, int limit = 100) => string.Format(SCHEMA_FORMAT, offset, limit);
		public const string SCHEMA_FORMAT = """
		SELECT DISTINCT ?st ?p ?ot WHERE {{
			?s a ?st .
			OPTIONAL {{
				?s ?p ?o .
				OPTIONAL {{
					?o a ?ot .
				}}
			}}
		}}
		OFFSET {0}
		LIMIT {1}
		""";

		public static string GraphSchemaClassesQuery(string graph, int offset = 0, int limit = 100) => string.Format(GRAPH_SCHEMA_CLASSES_FORMAT, graph, offset, limit);
		public const string GRAPH_SCHEMA_CLASSES_FORMAT = """
		SELECT DISTINCT ?st (count(?s) as ?count) WHERE {{
		  GRAPH <{0}> {{
		    ?s a ?st .
		  }}
		}}
		GROUP BY ?st
		OFFSET {1}
		LIMIT {2}
		""";

		public static string GraphSchemaBatchDescribeQuery(IEnumerable<string> iris) => string.Format(GRAPH_SCHEMA_BATCH_DESCRIBE_CLASS_INSTANCE_FORMAT,  iris.ToIriList());
		public const string GRAPH_SCHEMA_BATCH_DESCRIBE_CLASS_INSTANCE_FORMAT = """
		SELECT DISTINCT ?p (IF(BOUND(?ot), ?ot, IF(ISIRI(?o), "FIXED", "LITERAL")) AS ?type) WHERE {{
			VALUES ?iris {{{0}}}
			BIND (?iris as ?s)
			?s ?p ?o .
		  OPTIONAL {{
		  	?o a ?ot
			}}
		}}
		""";

		public static string GraphSchemaClassInstancesQuery(string graph, string className, int offset = 0, int limit = 100) => string.Format(GRAPH_SCHEMA_CLASS_INSTANCES_FORMAT, graph, className, offset, limit);
		public static string GRAPH_SCHEMA_CLASS_INSTANCES_FORMAT = """
		SELECT ?s WHERE {{
		  GRAPH <{0}> {{
		    ?s a <{1}> .
		  }}
		}}
		OFFSET {2}
		LIMIT {3}
		
		""";

		public static string GraphSchemaPropertiesQuery(string graph, string className, int offset = 0, int limit = 100) => string.Format(GRAPH_SCHEMA_PROPERTIES_FORMAT, graph, className, offset, limit);
		public const string GRAPH_SCHEMA_PROPERTIES_FORMAT = """
		SELECT DISTINCT ?p (IF(BOUND(?ot), ?ot, IF(ISIRI(?o), "FIXED", "LITERAL")) AS ?type) WHERE {{
			GRAPH <{0}> {{
				?s a <{1}> .
		    ?s ?p ?o .
		    OPTIONAL {{
					?o a ?ot
		    }}
			}}
		}}
		OFFSET {2}
		LIMIT {3} 
		""";

		public static string GraphSchemaQuery(string graph, int offset = 0, int limit = 100) => string.Format(GRAPH_SCHEMA_FORMAT, graph, offset, limit);
		public const string GRAPH_SCHEMA_FORMAT = """
		SELECT DISTINCT ?st ?p ?ot WHERE {{
			GRAPH <{0}> {{
				?s a ?st .
				OPTIONAL {{
					?s ?p ?o .
					OPTIONAL {{
						?o a ?ot .
					}}
				}}
			}}
		}}
		OFFSET {1}
		LIMIT {2}
		""";

		public static string DescribeQuery(string iri) => string.Format(DESCRIBE_QUERY, iri);
		public const string DESCRIBE_QUERY = "DESCRIBE <{0}>";



		public static string RelatedTriplesQuery(IEnumerable<string> iris, IEnumerable<string> predicates, int limit = 25) => string.Format(RELATED_QUERY_FORMAT, iris.ToIriList(), predicates.ToIriList(), limit);
		public const string RELATED_QUERY_FORMAT = """
		SELECT ?s ?p ?o WHERE {{		
			VALUES ?iris {{{0}}}
			VALUES ?p {{{1}}}
			{{
				BIND(?iris AS ?s)
				BIND(?iris AS ?o)
				?s ?p ?o .
			}}
		}}
		ORDER BY IF(isLiteral(?o), 0, 1)
		LIMIT {2}
		""";

	}
}
