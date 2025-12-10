@tool
extends Control

@export var logs :ItemList
@export var detail :RichTextLabel
@export var clear_button :Button;
@export var info_button :Button;
@export var warn_button :Button;
@export var error_button :Button;	
@export var search_edit :LineEdit;
@export var search_timer :Timer;
@export var close_button :Button;
@export var info_icon :Texture2D;
@export var warn_icon :Texture2D;
@export var error_icon :Texture2D;
@export var clear_icon :Texture2D;
@export var search_icon :Texture2D;

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
	
signal close;	
	
var log_data_list :Array[LogData] = [];
var full_log_data_list :Array[LogData] = [];
var categorys :Array[String] = [];
var id:int;
var config:ConfigFile = ConfigFile.new();
var is_editor := false;
var click_code_link_callback:Callable;

var log_select_index := -1;
var log_select_time_stamp := -1;

var info_log_count := 0;
var warn_log_count := 0;
var error_log_count := 0;

func init(id:int,is_editor:bool = false,click_code_link_callback:Callable = Callable())->void:
	self.id = id;
	self.is_editor = is_editor;
	self.click_code_link_callback = click_code_link_callback;
	self._load_config();

func _ready() -> void:
	logs.item_selected.connect(self._on_log_selected)	
	self.close_button.pressed.connect(self._on_click_close);
	self.info_button.toggled.connect(self._on_toggle_info);
	self.warn_button.toggled.connect(self._on_toggle_warn);
	self.error_button.toggled.connect(self._on_toggle_error);
	self.clear_button.pressed.connect(self.clear);
	self.search_edit.text_changed.connect(self._on_search_changed);
	self.search_timer.timeout.connect(self._refresh);
	if self.is_editor:
		self.detail.meta_clicked.connect(self.click_code_link_callback);
	
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
				
func add_logs(data:Array):
	var i := 0;
	while i < data.size():
		self.add_log(data[i],data[i+1],data[i+2],data[i+3]);
		i += 4;	
			
func add_category(category:String):
	pass	

func add_categories(array:Array):
	for category in array:
		self.add_category(category);		
	
func clear():
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
	
#region for C#
func show_close():
	self.close_button.visible = true;		
#endregion
	
func _load_config():
	self.config.load("user://log_profiler.cfg");
	self.info_button.button_pressed = self.config.get_value(str(self.id),"info_switch",true);
	self.warn_button.button_pressed = self.config.get_value(str(self.id),"warn_switch",true);
	self.error_button.button_pressed = self.config.get_value(str(self.id),"error_switch",true);
	
func _refresh():
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
	
func _on_toggle_info(open:bool):
	self.config.set_value(str(self.id),"info_switch",open);
	self._refresh();
	
func _on_toggle_warn(open:bool):
	self.config.set_value(str(self.id),"warn_switch",open);
	self._refresh();
	
func _on_toggle_error(open:bool):
	self.config.set_value(str(self.id),"error_switch",open);
	self._refresh();
	
func _on_search_changed(content:String):
	self.search_timer.start(self.search_timer.wait_time);
	
func _on_log_selected(index:int):		
	if index != self.log_select_index:
		self.detail.text = self.log_data_list[index].stack;	
		self.log_select_time_stamp = Time.get_unix_time_from_system();
		self.log_select_index = index;
	elif self.is_editor:
		var time_stamp: float = Time.get_unix_time_from_system();	
		if time_stamp - self.log_select_time_stamp < 1:
			var stack: String = self.log_data_list[index].stack;
			var regex = RegEx.new();
			regex.compile(r'\[url=({.*})\]');
			var url = regex.search(stack).get_string(1);
			if !url.is_empty():
				self.click_code_link_callback.call(url);
		self.log_select_time_stamp = Time.get_unix_time_from_system();
		
func _on_click_close():
	self.close.emit();
	self.queue_free();
