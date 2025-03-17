$.validator.addMethod("salaryRange", function (value, element) {
    return this.optional(element) || new RegExp("^\\d+(\\.\\d{1,2})?$").test(value);
}, "Enter a valid salary (numbers and up to 2 decimal places only)");

$.validator.addMethod("EmailFormate", function (value, element) {
    return this.optional(element) || new RegExp("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$").test(value);
}, "Enter a valid Email only");

$.validator.addMethod("AlphabelsFormate", function (value, element) {
    return this.optional(element) || new RegExp("^[a-zA-Z\\s]+$").test(value);
}, "Accepts only alphabets and spaces");
