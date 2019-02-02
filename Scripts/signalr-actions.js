window.onload = function () {    
    var loadTime;
    var startCalc = -1, endCalc = -1;
    // Reference the auto-generated proxy for the hub.
    var stream = $.connection.auctionChanges;
    

    $(function () {        
 
        function Countdown(options) {            

            var timer,
            instance = this,
            idElement = options.id,
            seconds = options.seconds || 10,
            updateStatus = options.onUpdateStatus || function () { },
            counterEnd = options.onCounterEnd || function () { };

            function decrementCounter() {                
                var timeStr = document.getElementById(idElement).getAttribute('data-value');                
                seconds = Math.round(parseFloat(timeStr));
                updateStatus(seconds);                
                if (seconds === 0) {
                    counterEnd();
                    instance.stop();
                }                
                seconds--;                
                document.getElementById(idElement).setAttribute('data-value', seconds + "");                
            }

            this.start = function () {                
                clearInterval(timer);                
                timer = 0;                
                seconds = options.seconds;                
                timer = setInterval(decrementCounter, 1000);

            };

            this.stop = function () {
                clearInterval(timer);
            };
        }        

        function startTimer(certainObject, ifLoadTime) {

            var elem = certainObject;

            var fullID = elem.id;
            var res = fullID.split('_');
            var onlyAuctionID = res[0];

            var statusID = onlyAuctionID + '_status';
            var status = document.getElementById(statusID).innerHTML;

            var timeID = onlyAuctionID + '_time';
            var sec = elem.getAttribute('data-value');
            if (ifLoadTime == true) sec = sec - loadTime;            
            
            sec = Math.round(parseFloat(sec));            
            elem.setAttribute('data-value', sec + "");

            if (sec > 0) {
    
                if (status == 'OPEN') {
    
                    var myCounter = new Countdown({
                        id: timeID,
                        seconds: sec, // number of seconds to count down
                        onUpdateStatus: function (sec) {
                                                        
                            var totalSec = sec;
                            var hours = parseInt(totalSec / 3600) % 24;
                            var minutes = parseInt(totalSec / 60) % 60;
                            var seconds = totalSec % 60;

                            var result = (hours < 10 ? "0" + hours : hours) + ":" + (minutes < 10 ? "0" + minutes : minutes) + ":" + (seconds < 10 ? "0" + seconds : seconds);

                            elem.innerHTML = result;
                        }, // callback for each second
                        onCounterEnd: function () {
    
                            elem.innerHTML = '';
                            // Start the connection.
                            $.connection.hub.start().done(function () {                             
                                stream.server.changeAuctionStatusOver(onlyAuctionID);
                            });
                        } // final action
                    });
                    
                    myCounter.start();
                }
            }
            else {
  
                elem.innerHTML = '';
                $.connection.hub.start().done(function () {                    
                    stream.server.changeAuctionStatusOver(onlyAuctionID);
                });
            }
        }        
  
        // Create a function that the hub can call back to display messages.
        stream.client.updateLastBidHome = function (userID, auctionID, fullName, newPrice, noTokens, timeRemaining) {
            
            if (timeRemaining > 0) {       
                if (noTokens == true) {
                    var userIDTemp = $('#userIDLabel').val();
                    if (userID == userIDTemp) {
                        var start = Date.now();
                        var timeID = auctionID + '_time';
                        var time = document.getElementById(timeID).getAttribute('data-value');
                        time = Math.round(parseFloat(time));
                        alert("Nemate dovoljno tokena. Molimo Vas, kupite tokene kako bi nastavili sa aukcijom.");                        
                        var diff = (Date.now() - start) / 1000;                        
                        time = time - diff;                        
                        document.getElementById(timeID).setAttribute('data-value', time + '');                        
                    }                        
                }                    
                else {
                    
                    var fieldToChangeID = '#' + auctionID + '_lastBid';                    
                    var fieldToChangePriceID = '#' + auctionID + '_lastPrice';                    
                    var timeID = auctionID + '_time';
                    
                    var tokensID = '#' + userID + '_tokens';                    
                    var tokensStr = $(tokensID).text();                    
                    var tokens = parseInt(tokensStr) - 1;

                    $(tokensID).text(tokens + '');
                    document.getElementById(timeID).setAttribute('data-value', timeRemaining + '');
                    
                    $(fieldToChangeID).text(fullName);                    
                    $(fieldToChangePriceID).text(newPrice);                    
                }                
            } else {
                var userIDTemp = $('#userIDLabel').val();
                if (userID == userIDTemp)
                    alert("Aukcija je završena.");
            }                
        };              

        stream.client.changeStartPriceAdmin = function (auctionID, newPrice) {

            var fieldToChangeStartPriceID = '#' + auctionID + '_startPrice';
            var fieldToChangePriceID = '#' + auctionID + '_lastPrice';

            $(fieldToChangeStartPriceID).text(newPrice);
            $(fieldToChangePriceID).text(newPrice);
        };         

        stream.client.auctionOpened = function (auctionID, str) {
            
            var adminWrapper = '#admin-wrapper-' + auctionID;
            var userWrapper = '#user-wrapper-' + auctionID;

            $(adminWrapper).css('display', 'none');
            $(userWrapper).css('display', 'inline');
            
            var btnID = '#' + auctionID + '_btn';
            $(btnID).css('display' , 'inline');
            
            var statusID = '#' + auctionID + '_status'; 
            $(statusID).text('OPEN');
            $(statusID).removeClass().addClass('aa-badge aa-sale');

            var timeID = auctionID + "_time";
            var obj = document.getElementById(timeID);                                          
  
            /*
            endCalc = Date.now();            
            if (startCalc == -1) startCalc = str;
                        
            loadTime = (endCalc - startCalc) / 1000;
            */

            loadTime = str;
            startTimer(obj, true);

            startCalc = -1;            
            endCalc = -1;
        };        
  
        stream.client.auctionStatusChangedOver = function (auctionID, newStatus) {

            var badgeClass;
            var adminWrapper = '#admin-wrapper-' + auctionID;
            var userWrapper = '#user-wrapper-' + auctionID;

            switch (newStatus) {
                case ("OPEN"):
                    badgeClass = "aa-badge aa-sale";
                    break;
                case ("READY"):
                    badgeClass = "aa-badge aa-hot";
                    break;
                default:
                    badgeClass = "aa-badge aa-sold-out";
                    break;
            }

            $(adminWrapper).css('display', 'none');
            $(userWrapper).css('display', 'none');

            var statusID = '#' + auctionID + '_status';
            var buttonID = '#' + auctionID + '_btn';
                        
            $(statusID).text(newStatus);
            $(statusID).removeClass().addClass(badgeClass);
            $(buttonID).css('display', 'none');            
        };        
  
        // Start the connection.
        $.connection.hub.start().done(function () {            
            $('.bidBtn').click(function () {                                

                var auctionID = this.getAttribute('data-value');
                var userID = $('#userIDLabel').val();

                // Call the SendBid method on the hub.
                stream.server.sendBid(auctionID, userID);
            });

            $('#changePriceBtn').click(function () {

                var auctionID = this.getAttribute('data-value');
                var newPrice = $('#newStartPriceLabel').val();

                if ((newPrice == null) || (newPrice == ''))
                    alert('Unesite novu početnu cenu aukcije.');
                else
                    stream.server.changeStartPrice(auctionID, newPrice);
            });

            $('#activateAuction').click(function () {
                
                startCalc = Date.now();
                var auctionID = this.getAttribute('data-value');
                stream.server.activateAuction(auctionID, startCalc);
            });
        });        

        loadTime = window.performance.timing.domContentLoadedEventEnd - window.performance.timing.navigationStart;        
        loadTime = loadTime / 1000;

        $('.time-remaining').each(function (i, obj) {

            var fullIDTemp = this.id;
            var resTemp = fullIDTemp.split("_");
            var onlyAuctionIDTemp = resTemp[0];

            var statusIDTemp = onlyAuctionIDTemp + "_status";
            var statusTemp = document.getElementById(statusIDTemp).innerHTML;

            if (statusTemp == 'OPEN') startTimer(this, true);
            else startTimer(this, false);

        });
    });
}