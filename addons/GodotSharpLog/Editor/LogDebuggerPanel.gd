@tool
extends Control

@export var logs:ItemList
@export var detail:RichTextLabel
@export var clear_button:Button;
@export var info_button:Button;
@export var warn_button:Button;
@export var error_button:Button;	
@export var search_edit:LineEdit;
@export var search_timer :Timer;

enum CategoryFilterType {
	None,
	Contains,
	Exclude,
}
	
class LogData:
	var level:int;
	var category:String;
	var content:String;
	var stack:String;
	
var log_data_list :Array[LogData] = [];
var full_log_data_list :Array[LogData] = [];
var categorys :Array[String] = [];
var session_id:int;
var session:EditorDebuggerSession;
var config:ConfigFile = ConfigFile.new();

var info_icon := EditorInterface.get_editor_theme().get_icon("Popup","EditorIcons");
var warn_icon := EditorInterface.get_editor_theme().get_icon("StatusWarning","EditorIcons");
var error_icon := EditorInterface.get_editor_theme().get_icon("StatusError","EditorIcons");
var clear_icon := EditorInterface.get_editor_theme().get_icon("Clear","EditorIcons");
var search_icon := EditorInterface.get_editor_theme().get_icon("Search","EditorIcons");

var log_select_index := -1;
var log_select_time_stamp := -1;

var info_log_count := 0;
var warn_log_count := 0;
var error_log_count := 0;

func _ready() -> void:
	logs.item_selected.connect(self._on_log_selected)	
	self.detail.meta_clicked.connect(self._on_click_code_link);
	self.info_button.toggled.connect(self._on_toggle_info);
	self.warn_button.toggled.connect(self._on_toggle_warn);
	self.error_button.toggled.connect(self._on_toggle_error);
	self.clear_button.pressed.connect(self._clear);
	self.search_edit.text_changed.connect(self._on_search_changed);
	self.search_timer.timeout.connect(self._refresh);
	
	self.info_button.icon = info_icon;
	self.warn_button.icon = warn_icon;
	self.error_button.icon = error_icon;
	self.clear_button.icon = clear_icon;
	self.search_edit.right_icon = search_icon;
	
	self._load_config();

func init(id:int,session:EditorDebuggerSession)->void:
	self.session = session;
	self.session_id = id;
	self.session.toggle_profiler("gd_log",true);	
	
func start():
	print("session start")
	self._clear();
	self.session.toggle_profiler("gd_log",true);	
	
func add_log(level:int,category:String,content:String,stack:String):
	var data = LogData.new();
	data.level = level;
	data.category = category;
	if category != null and !category.is_empty():
		data.content = "["+category+"]"+content;
	else: 
		data.content = content;	
	data.stack = data.content + "\n" + stack;
	
	self.full_log_data_list.push_back(data);
	
	var icon : Texture2D;
	match level:
		0:
			icon = info_icon;
			self.info_log_count += 1;
			self.info_button.text = str(self.info_log_count);
			if self.info_button.button_pressed:
				self.log_data_list.push_back(data);
				logs.add_item(content,info_icon);
		1:
			icon = warn_icon;
			self.warn_log_count += 1;
			self.warn_button.text = str(self.warn_log_count);
			if self.warn_button.button_pressed:
				self.log_data_list.push_back(data);
				logs.add_item(content,warn_icon);
		2,3:
			icon = error_icon;
			self.error_log_count += 1;
			self.error_button.text = str(self.error_log_count);
			if self.error_button.button_pressed:
				self.log_data_list.push_back(data);
				logs.add_item(content,error_icon);
			
func add_category(category:String):
	pass	
	
func _load_config():
	self.config.load("user://log_profiler.cfg");
	self.info_button.button_pressed = self.config.get_value(str(self.session_id),"info_switch",true);
	self.warn_button.button_pressed = self.config.get_value(str(self.session_id),"warn_switch",true);
	self.error_button.button_pressed = self.config.get_value(str(self.session_id),"error_switch",true);
	
func _refresh():
	print(111)
	var start: float = Time.get_unix_time_from_system();
	self.log_data_list.clear();
	
	for data in self.full_log_data_list:
		if self._filter(data):
			self.log_data_list.push_back(data);
	
	start = Time.get_unix_time_from_system();
	self.logs.clear();
	for data in self.log_data_list:
		match data.level:
			0:
				self.logs.add_item(data.content,self.info_icon);
			1:
				self.logs.add_item(data.content,self.warn_icon);
			2,3:
				self.logs.add_item(data.content,self.error_icon);
	
func _filter(data:LogData)->bool:
	match data.level:
		0:
			if !self.info_button.button_pressed: 
				return false;
		1:
			if !self.warn_button.button_pressed:
				return false;
		2,3:
			if !self.error_button.button_pressed:
				return false;
				
	if !self.search_edit.text.is_empty() and !data.content.containsn(self.search_edit.text):
		return false;
	
	return true;	
	
func _clear():
	self.logs.clear();
	self.log_data_list.clear();
	self.full_log_data_list.clear();
	self.detail.text = "";
	
	self.info_log_count = 0;
	self.warn_log_count = 0;
	self.error_log_count = 0;
	
	self.info_button.text = "0";
	self.warn_button.text = "0";
	self.error_button.text = "0";
	
func _on_toggle_info(open:bool):
	self.config.set_value(str(self.session_id),"info_switch",open);
	self._refresh();
	
func _on_toggle_warn(open:bool):
	self.config.set_value(str(self.session_id),"warn_switch",open);
	self._refresh();
	
func _on_toggle_error(open:bool):
	self.config.set_value(str(self.session_id),"error_switch",open);
	self._refresh();
	
func _on_search_changed(content:String):
	self.search_timer.start(self.search_timer.wait_time);
	
func _on_log_selected(index:int):
	if index != self.log_select_index:
		self.detail.text = self.log_data_list[index].stack;	
		self.log_select_time_stamp = Time.get_unix_time_from_system();
		self.log_select_index = index;
	else:
		var time_stamp: float = Time.get_unix_time_from_system();	
		print(time_stamp - self.log_select_time_stamp)
		if time_stamp - self.log_select_time_stamp < 0.3:
			var stack: String = self.log_data_list[index].stack;
			var regex = RegEx.new();
			regex.compile(r'\[url=({.*})\]');
			var url = regex.search(stack).get_string(1);
			if !url.is_empty():
				self._on_click_code_link(url);
		self.log_select_time_stamp = Time.get_unix_time_from_system();	
	
func _on_click_code_link(json):
	var param = JSON.parse_string(json);
	var script: Script = load("res://"+param.path);
	EditorInterface.edit_script(script,param.line);	
