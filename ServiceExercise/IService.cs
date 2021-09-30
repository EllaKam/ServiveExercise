using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceExercise
{
    public interface IService
    {
        void sendRequest(Request request);
        long getSummary();
    }
}
