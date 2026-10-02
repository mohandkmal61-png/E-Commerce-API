using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerse.Sharde.CommenResult
{
    public class Result
    {
        protected readonly List<Error> _errors = new List<Error>();

        public bool IsSuccess => !_errors.Any();

        public bool IsFailure => _errors.Any();


        public IReadOnlyList<Error> Errors => _errors.AsReadOnly();


        protected Result()
        {

        }


        protected Result(Error error)
        {
            _errors.Add(error);
        }

        protected Result(IEnumerable<Error> errors)
        {
            _errors.AddRange(errors);
        }



        public static Result ok()
        {
            return new Result();
        }

        public static Result fail(Error error)
        {
            return new Result(error);
        }


        public static Result fail(IEnumerable<Error> errors)
        {
            return new Result(errors);
        }


    }


    public class Result<TValue> : Result
    {

        private readonly TValue _value;


        public TValue Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access the value of a failed result.");


        private Result(TValue value)
        {
            _value = value;

        }

        private Result(Error error) : base(error)
        {
            _value = default!;

        }

        private Result(List<Error> errors) : base(errors)
        {
            _value = default!;
        }

        public static Result<TValue> ok(TValue value)
        {
            return new Result<TValue>(value);
        }

        public new static Result<TValue> fail(Error error)
        {
            return new Result<TValue>(error);
        }

        public static Result<TValue> fail(List<Error> errors)
        {
            return new Result<TValue>(errors);

        }
    }

    }
