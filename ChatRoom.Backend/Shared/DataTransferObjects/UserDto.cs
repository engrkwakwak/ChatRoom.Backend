using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects {
    public record UserDto {
        public int UserId { get; init; }

        // other properties...
    }
}


/*
 * Instead of a regular class, we are using a record for DTO. 
 * This specific record type is known as a Positional record. 
 * A Record type provides us an easier way to create an immutable reference type in .NET. 
 * This means that the Record’s instance property values cannot change after its initialization. 
 * The data are passed by value and the equality between two Records is verified by comparing the value of their properties. 
 * Records can be a valid alternative to classes when we have to send or receive data. 
 * The very purpose of a DTO is to transfer data from one part of the code to another, and immutability in many cases is useful. 
 * We use them to return data from a Web API or to represent events in our application. 
 * This is the exact reason why we are using records for our DTOs.
 */