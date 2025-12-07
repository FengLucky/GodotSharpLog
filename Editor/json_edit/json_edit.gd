@tool
extends Control

enum FieldType{
	STRING = 0, # 字符串
	NUMBER = 1, # 数字
	BOOLEAN = 2, # 布尔
	JSON = 3, # Json
}

enum OperatorType{
	ADD = 0, # 添加
	MODIFY = 1, # 修改
	DELETE = 2, # 删除
}

enum ConditionType{
	NONE = 0, #  无
	EQUAL = 1, # 等于
	NOT_EQUAL = 2, # 不等于
	GREATER = 3, # 大于
	LESS = 4, # 小于
	GREATER_EQUAL = 5, # 大于等于
	LESS_EQUAL = 6, # 小于等于
	FUNCTION = 7, # 函数
}

@export var dir_path:LineEdit;
@export var path_selector:Button;
@export var wildcard:LineEdit;
@export var field_name:LineEdit;
@export var operator:OptionButton; # 0 修改 1 删除
@export var field_type:OptionButton; # 0 字符串 1 数字 2 布尔 3 Json
@export var condition:OptionButton; # 0 无 1 等于 2 不等于 3 大于 4 小于 5 大于等于 6 小于等于 7 函数
@export var target_value_root:Control;
@export var target_value:LineEdit;
@export var new_value_root:Control;
@export var new_value:LineEdit;
@export var function:TextEdit;
@export var execute:Button;
@export var result:RichTextLabel;
@export var file_dialog:FileDialog;

var _files_ready := false;
var _field_ready := false;
var _operator_ready := false; 

func _ready() -> void:
	dir_path.text_changed.connect(_refresh_files);
	wildcard.text_changed.connect(_refresh_files);
	field_name.text_changed.connect(_refresh_field_name);
	operator.item_selected.connect(_refresh_operator);
	field_type.item_selected.connect(_refresh_operator);
	condition.item_selected.connect(_refresh_operator);
	target_value.text_changed.connect(_refresh_value);
	new_value.text_changed.connect(_refresh_value);
	function.text_changed.connect(_refresh_operator);
	
	path_selector.pressed.connect(_on_path_selector_pressed);
	execute.pressed.connect(_on_execute_pressed);
	
	file_dialog.dir_selected.connect(_on_file_dialog_selected);
	
	_refresh();

func _refresh() -> void:
	_refresh_files("");
	_refresh_field_name("");
	_refresh_operator(-1);
	
func _refresh_files(_value:String) -> void:
	var files := _get_handle_files();
	var log_str := "";
	for file in files:
		log_str += file + "\n";
	
	if files.size() > 0:
		result.text = log_str;
	else:
		result.text = "没有匹配的文件"
		
	_files_ready = files.size() > 0;
	_refresh_execute();
	
func _refresh_field_name(_value:String) -> void:
	_field_ready = not field_name.text.strip_edges().is_empty();
	_refresh_execute();
	
func _refresh_value(_value:String) -> void:
	_refresh_operator(-1)
	
func _refresh_operator(_value:int) -> void:
	var json_type := field_type.selected == FieldType.JSON;
	var number_type := field_type.selected == FieldType.NUMBER;
	var none_condition := condition.selected == ConditionType.NONE;
	var function_condition := condition.selected == ConditionType.FUNCTION;
	var add_operator := operator.selected == OperatorType.ADD;
	var modify_operator := operator.selected == OperatorType.MODIFY;

	target_value_root.visible = not none_condition and not function_condition and not json_type;
	new_value_root.visible = add_operator or modify_operator;
	function.visible = function_condition;
	
	condition.set_item_disabled(ConditionType.EQUAL,json_type or add_operator);
	condition.set_item_disabled(ConditionType.NOT_EQUAL,json_type or add_operator);
	condition.set_item_disabled(ConditionType.GREATER,json_type or not number_type or add_operator);
	condition.set_item_disabled(ConditionType.LESS,json_type or not number_type or add_operator);
	condition.set_item_disabled(ConditionType.GREATER_EQUAL,json_type or not number_type or add_operator);
	condition.set_item_disabled(ConditionType.LESS_EQUAL,json_type or not number_type or add_operator);

	match condition.selected:
		ConditionType.NONE:
			_operator_ready = true;	
		ConditionType.FUNCTION:
			_operator_ready = not function.text.strip_edges().is_empty();
		_:
			_operator_ready = not target_value.text.strip_edges().is_empty();

	if add_operator or modify_operator:
		_operator_ready = _operator_ready and not new_value.text.strip_edges().is_empty();
	
	_refresh_execute();

func _refresh_execute() -> void:
	var execute_ready := _files_ready and _field_ready  and _operator_ready;
	execute.disabled = not execute_ready;
		
func _on_path_selector_pressed() -> void:
	file_dialog.popup_centered();
	
func _on_file_dialog_selected(dir) -> void:
	dir_path.text = dir;
	_refresh_files(dir);
	
func _on_execute_pressed() -> void:
	var files := _get_handle_files();
	var log_str := "";
	var field_name_array := field_name.text.strip_edges().split(".");
	var new_value_text := new_value.text.strip_edges();
	var target_value_text := target_value.text.strip_edges();
	var condition_value:Variant;
	var value:Variant;
	var custom_function:Callable;
	var need_condition_value := operator.selected != OperatorType.ADD and condition.selected != ConditionType.NONE and condition.selected != ConditionType.FUNCTION;
	match field_type.selected:
		FieldType.STRING:
			if need_condition_value:
				condition_value = target_value_text;
			value = target_value_text;
		FieldType.NUMBER:
			if need_condition_value:
				if not target_value_text.is_valid_float():
					result.text = "[color=red]目标值不是合法 float[/color]";
					return;
				condition_value = target_value.text.to_float();
			
			if not new_value_text.is_valid_float():
				result.text = "[color=red]新值不是合法 float[/color]";
				return;
			value = new_value.text.to_float();
		FieldType.BOOLEAN:
			if need_condition_value:
				var condition_bool_value := _parse_string_to_bool(target_value_text);
				if condition_bool_value < 0:
					result.text = "[color=red]目标值不是合法 bool [true,false,1,0][/color]";
					return;
				condition_value = condition_bool_value > 0;
			
			var bool_value := _parse_string_to_bool(new_value_text);
			if bool_value < 0:
				result.text = "[color=red]新值不是合法 bool [true,false,1,0][/color]";
				return;
			value = bool_value > 0;
		FieldType.JSON:
			value = JSON.parse_string(new_value_text);
			if value == null:
				result.text = "[color=red]新值不是合法 json[/color]";
				return;
		_:
			result.text = "[color=red]未实现的字段类型："+str(field_type.selected)+"[/color]";
			return;
			
	if condition.selected == ConditionType.FUNCTION:
		var script := GDScript.new();
		script.source_code = "static " + function.text.strip_edges();
		script.reload();
		var methods := script.get_script_method_list()
		if methods.size() == 0:
			result.text = "[color=red]自定义函数编译错误[/color]";
			return;
		custom_function = Callable.create(script,methods[0]["name"])

	for file in files:
		log_str += file;
		var error := "";
		var hadle_count := 0;
		
		var json_string := FileAccess.get_file_as_string(file);
		if json_string.is_empty():
			log_str += "\n[color=red]文件内容为空[/color]\n"
			continue;
		
		var json :Variant = JSON.parse_string(json_string);
		if json == null:
			log_str += "\n[color=red]解析失败，不是 json 格式文件[/color]\n"
			continue;
		
		if json is not Array:
			log_str += "\n[color=red]解析失败，不是 json 数组[/color]\n"
			continue;
		
		for obj in json:
			var err := _handle_one_record(obj, field_name_array,value,condition_value,custom_function);
			if err.is_empty():
				hadle_count += 1;
			else:
				error += "[color=red]"+err+"[/color]\n"+JSON.stringify(obj,"\t")+"\n"
			
		var access := FileAccess.open(file,FileAccess.WRITE);
		access.store_string(JSON.stringify(json,"\t"))
		if access.get_error() != Error.OK:
			error = "保存文件到 "+file+" 失败\n" + error; 
		log_str += "\t\t\t\t\t成功处理 "+str(hadle_count)+" 个配置\n"
		if not error.is_empty():
			log_str += "[color=red]"+error+"[/color]\n"
	result.text = log_str;
	
func _handle_one_record(obj:Dictionary, field_name_array:Array[String],value:Variant,condition_value:Variant,custom_function:Callable) -> String:
	var base_field:Dictionary
	var field := field_name_array[field_name_array.size() - 1];
	if field_name_array.size() > 1:
		for i in field_name_array.size() - 1:
			var v = obj.get(field_name_array[i])
			if v == null:
				return "父字段 "+_get_field_base_path(field_name_array)+" 不存在"
			else: if v is not Dictionary:
				return "父字段 "+_get_field_base_path(field_name_array)+" 不是对象"
			base_field = v as Dictionary;
	else:
		base_field = obj;
		
	if not operator.selected == OperatorType.DELETE and base_field.has(field) and not _check_field_type(base_field[field],field_type.selected):
		return "原字段类型为 "+type_string(typeof(base_field[field])) + "，与目标类型 "+field_type.get_item_text(field_type.selected) + " 不一致"
		
	if not operator.selected == OperatorType.ADD and not condition.selected == ConditionType.NONE:
		if condition.selected != ConditionType.FUNCTION and not _check_condition(base_field[field],condition_value,condition.selected):
			return "";
		else: if not custom_function.call(obj):
			return "";
	
	if operator.selected == OperatorType.ADD:
		base_field.get_or_add(field,value)
	else: if operator.selected == OperatorType.MODIFY:
		if base_field.has(field):
			base_field[field] = value;
	else:
		base_field.erase(field);
	return ""
	
func _check_condition(value:Variant,condition_value:Variant,condition_type:ConditionType) ->bool:
	print("value:"+str(value)+" condition_value:"+str(condition_value) +" condition_type:"+condition.get_item_text(condition_type))
	match condition_type:
		ConditionType.EQUAL:
			return value == condition_value;
		ConditionType.NOT_EQUAL:
			return value != condition_value;
		ConditionType.GREATER:
			if value == null or condition_value == null:
				return false;
			return value > condition_value;
		ConditionType.LESS:
			if value == null or condition_value == null:
				return false;
			return value < condition_value;
		ConditionType.GREATER_EQUAL:
			if value == null or condition_value == null:
				return false;
			return value >= condition_value;
		ConditionType.LESS_EQUAL:
			if value == null or condition_value == null:
				return false;
			return value <= condition_value;
		_:
			return false;
	
func _check_field_type(obj:Variant,type:FieldType) -> bool:
	match type:
		FieldType.STRING:
			return obj is String;
		FieldType.NUMBER:
			return obj is float;
		FieldType.BOOLEAN:
			return obj is bool;
		FieldType.JSON:
			return obj is Dictionary or obj is Array;
		_:
			return false;
	
func _parse_string_to_bool(str_value:String) -> int:
	match str_value.to_lower():
		"true":
			return 1;
		"false":
			return 0;
		"1":
			return 1;
		"0":
			return 0;
		_:
			return -1;
	
func _get_field_base_path(field_name_array:Array[String]) -> String:
	var field_path := ""
	for j in field_name_array.size() - 1:
		if j != 0:
			field_path += "."
		field_path += field_name_array[j]
	return field_path
	
func _get_handle_files() -> Array[String]:
	var match_result :Array[String] = [];
	var dir := dir_path.text.strip_edges();
	if dir.is_empty():
		return match_result;
	if not DirAccess.dir_exists_absolute(dir):
		return match_result;
	var pattern :String;
	if wildcard.text.is_empty():
		pattern = wildcard.placeholder_text;
	else:
		pattern = wildcard.text;
	pattern = pattern.strip_edges()
	var regex_str := wildcard_to_regex(pattern)
	var regex := RegEx.new()
	regex.compile(regex_str)
	__get_handle_files(dir,regex,match_result);
	return match_result

func __get_handle_files(path:String,regex:RegEx, match_result:Array[String]) -> void:
	for file in DirAccess.get_files_at(path):
		if regex.search(file):
			result.append(path+"/"+file)
			
	for dir in DirAccess.get_directories_at(path):
		__get_handle_files(dir,regex,match_result)

func wildcard_to_regex(pattern: String) -> String:
	# 转义所有正则特殊字符，但保留 * 和 ? 用于后续替换
	var escaped := pattern.replace("\\", "\\\\")
	escaped = escaped.replace("^", "\\^")
	escaped = escaped.replace("$", "\\$")
	escaped = escaped.replace(".", "\\.")
	escaped = escaped.replace("[", "\\[")
	escaped = escaped.replace("]", "\\]")
	escaped = escaped.replace("(", "\\(")
	escaped = escaped.replace(")", "\\)")
	escaped = escaped.replace("{", "\\{")
	escaped = escaped.replace("}", "\\}")
	escaped = escaped.replace("|", "\\|")
	escaped = escaped.replace("+", "\\+")
	
	# 现在把通配符 * 和 ? 转换为正则
	escaped = escaped.replace("*", ".*")
	escaped = escaped.replace("?", ".")
	
	# 锚定整个字符串（完全匹配）
	return "^" + escaped + "$"
