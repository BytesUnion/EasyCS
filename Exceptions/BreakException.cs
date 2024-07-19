class BreakException : Exception {
    public BreakException() : base("Can't use 'break' keyword outside loops")
    {
    }
}