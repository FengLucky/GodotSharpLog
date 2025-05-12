@tool
extends Control

@export var logs:ItemList
@export var detail:RichTextLabel

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
var session_id:int;
var session:EditorDebuggerSession;
var config:ConfigFile = ConfigFile.new();

var info_switch := true;
var warn_switch := true;
var error_switch := true;

var info_icon := EditorInterface.get_editor_theme().get_icon("Popup","EditorIcons");
var warn_icon := EditorInterface.get_editor_theme().get_icon("StatusWarning","EditorIcons");
var error_icon := EditorInterface.get_editor_theme().get_icon("StatusError","EditorIcons");

func init(id:int,session:EditorDebuggerSession)->void:
	self.session = session;
	self.session_id = session_id;
	self.session.toggle_profiler("gd_log",true);	
	
func start():
	print("session start")
	self._clear();
	self.session.toggle_profiler("gd_log",true);	
	
func _ready() -> void:
	logs.item_selected.connect(self._on_log_selected)	
	self.detail.connect("meta_clicked",self._on_click_code_link);
	
func add_log(level:int,category:String,content:String,stack:String):
	var data = LogData.new();
	data.level = level;
	data.category = category;
	data.content = content;
	data.stack = stack;	
	log_data_list.push_back(data);
	
	var icon : Texture2D;
	match level:
		0:
			icon = info_icon;
		1:
			icon = warn_icon;
		2:
			icon = error_icon;
		3:
			icon = error_icon;
			
	logs.add_item(content,icon);

func add_category(category:String):
	pass	
	
func _load_config():
	self.config.load("user://log_profiler.cfg");
	
func _clear():
	self.logs.clear();
	self.log_data_list.clear();
	
func _on_log_selected(index:int):
	self.detail.text = self.log_data_list[index].stack;	
	
func _on_click_code_link(json):
	var param = JSON.parse_string(json);
	var script = load("res://"+param.path) as Script;
	EditorInterface.edit_script(script,param.line);
