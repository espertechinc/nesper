use test;

drop procedure if exists spDelayTest;

delimiter $$

create procedure spDelayTest(timeout int, testValue int)
begin
	declare varout int;
	select sleep(timeout) into varout;
	select myint from mytesttable
	 where myint = testValue;
end$$
