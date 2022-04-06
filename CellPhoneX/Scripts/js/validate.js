function validateMail(email) {
    var a = document.getElementById(email).value;
    var filter = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})$/;
    if (filter.test(a)) {
        return true;
    }
    else {
        return false;
    }
}
function validatePhone(phone) {
    var a = document.getElementById(phone).value;
    var filter = /((^(\+84|84|0|0084){1})(3|5|7|8|9))+([0-9]{8})$/;
    if (filter.test(a)) {
        return true;
    }
    else {
        return false;
    }
}

function checkStrength(password) {
    var strength = 0
        if (password.length < 1) {
            $('#message').removeClass()
            $('#message').addClass('not null')
            return 'không được bỏ trống'
        }
        if (password.length < 6) {
            $('#message').removeClass()
            $('#message').addClass('Short')
            return 'Too short'
        }
        if (password.length > 7) strength += 1
        // If password contains both lower and uppercase characters, increase strength value.  
        if (password.match(/([a-z].*[A-Z])|([A-Z].*[a-z])/)) strength += 1
        // If it has numbers and characters, increase strength value.  
        if (password.match(/([a-zA-Z])/) && password.match(/([0-9])/)) strength += 1
        // If it has one special character, increase strength value.  
        if (password.match(/([!,%,&,@,#,$,^,*,?,_,~])/)) strength += 1
        // If it has two special characters, increase strength value.  
        if (password.match(/(.*[!,%,&,@,#,$,^,*,?,_,~].*[!,%,&,@,#,$,^,*,?,_,~])/)) strength += 1
        // Calculated strength value, we can return messages  
        // If value is less than 2  
        if (strength < 2) {
            $('#message').removeClass()
            $('#message').addClass('Weak')
            return 'Weak'
        } else if (strength == 2) {
            $('#message').removeClass()
            $('#message').addClass('Good')
            return 'Good'
        } else {
            $('#message').removeClass()
            $('#message').addClass('Strong')
            return 'Strong'
        }
    }
