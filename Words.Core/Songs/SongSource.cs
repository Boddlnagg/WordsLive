using System.Text.RegularExpressions;

namespace Words.Core.Songs
{
    public class SongSource
    {
        public string Songbook { get; set;}

        public int Number { get; set;}
        
        public static SongSource Parse(string source)
    	{
        	SongSource result = new SongSource();
        	
        	if(source.Trim() == "")
        		return result;
        	
	        bool success = false;
	        int n;
	        
	        if (source.Contains("/"))
	        {
	            var parts = source.Split('/');
	            result.Songbook = parts[0].Trim();
	            if (int.TryParse(parts[1].Trim(), out n))
	            {
	                result.Number = n;
	                success = true;
	            }
	        }
	
	        if (!success)
	        {
	        	int index = source.LastIndexOfAny(new char[] {' ','.',','});
	        	if (index >= 0 && int.TryParse(source.Substring(index+1).Trim(), out n))
	        	{
	        		result.Number = n;
	        		string book = source.Substring(0, index+1).Trim();
	        		if (book.EndsWith("Nr."))
	        			book = book.Substring(0, book.Length - 3).Trim();
	        		if (book.EndsWith(","))
	        			book = book.Substring(0, book.Length - 1).Trim();
	        		result.Songbook = book;
	        	}
	        	else
	        	{
	            	result.Songbook = source;
	        	}
	        }
	        
	        return result;
	    }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Songbook))
                return string.Empty;
            else if (Number == 0)
                return Songbook;
            else
                return Songbook + " / " + Number;
        }
    }
}